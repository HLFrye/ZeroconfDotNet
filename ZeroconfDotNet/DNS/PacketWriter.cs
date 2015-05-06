using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace ZeroconfDotNet.DNS
{
    public class PacketWriter : IDisposable
    {
        public static byte[] Write(Packet packet)
        {
            using (var stream = new MemoryStream())
            using (var writer = new PacketWriter(stream))
            {
                writer.WritePacket(packet);
                return stream.ToArray();                
            }
        }

        public PacketWriter(MemoryStream stream)
        {
            this.stream = stream;
            this.writer = new BinaryWriter(stream);
        }

        MemoryStream stream;
        BinaryWriter writer;
        NameCompressor compressor = new NameCompressor();

        public void WritePacket(Packet packet)
        {
            writer.Write(ByteOrder(packet.TransactionID));
            writer.Write(ByteOrder(packet.Flags));
            writer.Write(ByteOrder(packet.Questions));
            writer.Write(ByteOrder(packet.AnswerRRs));
            writer.Write(ByteOrder(packet.AuthorityRRs));
            writer.Write(ByteOrder(packet.AdditionalRRs));
            Write(packet.Queries);
            Write(packet.Answers);
            Write(packet.Authority);
            Write(packet.Additional);
            writer.Flush();
        }

        public void WriteString(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            writer.Write((byte)bytes.Length);
            writer.Write(bytes);
        }

        public long GetPosition()
        {
            return stream.Position;
        }        

        public void WriteAtPosition(UInt16 val, long pos)
        {
            stream.Seek(pos, SeekOrigin.Begin);
            WriteShort(val);
            stream.Seek(0, SeekOrigin.End);
        }

        public void WriteShort(UInt16 val)
        {
            writer.Write(ByteOrder(val));
        }

        public void WriteName(string name)
        {
            compressor.StoreName(writer, Encoding.UTF8, name);
        }

        void Write(IList<Query> queries)
        {
            foreach (var query in queries)
            {
                Write(query);
            }
        }

        void Write(Query query)
        {
            compressor.StoreName(writer, Encoding.UTF8, query.Record.Name);
            writer.Write(ByteOrder(query.Record.RecordType));
            writer.Write(ByteOrder(query.Record.Class));
        }

        void Write(IList<Answer> answers)
        {
            foreach (var answer in answers)
            {
                Write(answer);
            }
        }

        void Write(Answer answer)
        {
            compressor.StoreName(writer, Encoding.UTF8, answer.Record.Name);
            writer.Write(ByteOrder(answer.Record.RecordType));
            var rawClass = answer.Record.GetRawClass();
            writer.Write(ByteOrder(rawClass));
            writer.Write(ByteOrder(answer.TTL));
            answer.WriteData(this);
        }

        public void WriteIP6Address(IPAddress address)
        {
            var data = address.GetAddressBytes();
            writer.Write(data);
        }

        public void WriteIP4Address(IPAddress address)
        {
            var data = address.GetAddressBytes();
            writer.Write(data);
        }

        void Write(IList<Authority> authority)
        {
            if (authority.Count != 0)
                throw new NotImplementedException();
        }

        void Write(IList<Additional> additional)
        {
            if (additional.Count != 0)
                throw new NotImplementedException();
        }

        public void Dispose()
        {
            writer.Dispose();
        }

        public static UInt16 ByteOrder(UInt16 input)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (UInt16)(((input & 0xFF00) >> 8) | ((input & 0x00FF) << 8));
            }
            return input;
        }

        public static UInt32 ByteOrder(UInt32 input)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (UInt32)(((input & 0xFF000000) >> 24) | ((input & 0x00FF0000) >> 8) | ((input & 0x0000FF00) << 8) | ((input & 0x000000FF) << 24));
            }
            return input;
        }

        public class NameCompressor
        {
            public void StoreName(BinaryWriter writer, Encoding encoding, string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    writer.Write((byte)0);
                    return;
                }

                if (Strings.ContainsKey(name))
                {
                    var location = (UInt16)(Strings[name] | 0xc000);
                    writer.Write(ByteOrder(location));
                    return;
                }
                var offset = (UInt16)(writer.BaseStream.Position);
                Strings[name] = offset;
                var dotPos = name.IndexOf('.');
                if (dotPos != -1)
                {
                    var namepart = name.Substring(0, dotPos);
                    var nameBytes = encoding.GetBytes(namepart);
                    writer.Write((byte)nameBytes.Length);
                    writer.Write(nameBytes);
                    StoreName(writer, encoding, name.Substring(dotPos + 1));
                }
                else
                {
                    var nameBytes = encoding.GetBytes(name);
                    writer.Write((byte)nameBytes.Length);
                    writer.Write(nameBytes);
                    writer.Write((byte)0);
                }
            }

            Dictionary<string, UInt16> Strings = new Dictionary<string, ushort>();
        }
    }
}
