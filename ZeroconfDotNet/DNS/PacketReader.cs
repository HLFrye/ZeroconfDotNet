using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using ZeroconfDotNet.DNS.Records;

namespace ZeroconfDotNet.DNS
{
    public class PacketReader
    {
        NameExpander expander;
        BinaryReader reader;

        public PacketReader(BinaryReader reader)
        {
            expander = new NameExpander();
            this.reader = reader;
        }

        public static Packet Read(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                var packetReader = new PacketReader(reader);
                return packetReader.Read();
            }
        }

        public byte[] ReadBytes(int len)
        {
            return reader.ReadBytes(len);
        }

        public Packet Read()
        {
            var TransactionID = ByteOrder(reader.ReadUInt16());
            var Flags = reader.ReadUInt16();
            var Questions = ByteOrder(reader.ReadUInt16());
            var AnswerRRs = ByteOrder(reader.ReadUInt16());
            var AuthorityRRs = ByteOrder(reader.ReadUInt16());
            var AdditionalRRs = ByteOrder(reader.ReadUInt16());

            var Queries = ReadQueries(Questions);
            var Answers = ReadAnswers(AnswerRRs);
            var Authority = ReadAuthority(AuthorityRRs);
            var Additional = ReadAdditional(AdditionalRRs);
            return new Packet(TransactionID, Flags, Queries, Answers, Authority, Additional);
        }


        Query[] ReadQueries(uint count)
        {
            var result = new Query[count];
            for (int i = 0; i < count; ++i)
            {
                var record = ReadRecord();
                result[i] = new Query { Record = record };
            }
            return result;
        }

        private Record ReadRecord()
        {
            var name = ReadName();
            var typeId = ByteOrder(reader.ReadUInt16());
            var rclass = ByteOrder(reader.ReadUInt16());
            return new Record(name, typeId, rclass);
        }

        public string ReadString()
        {
            var len = reader.ReadByte();
            var bytes = reader.ReadBytes(len);
            return Encoding.UTF8.GetString(bytes);
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }

        Answer[] ReadAnswers(int count)
        {
            var result = new Answer[count];
            for (int i = 0; i < count; ++i)
            {
                var ans = new Answer();
                ans.Record = ReadRecord();
                ans.ReadData(this);

                result[i] = ans;
            }
            return result;
        }

        public UInt16 ReadShort(bool changeByteOrder = true)
        {
            var value = reader.ReadUInt16();
            if (changeByteOrder)
            {
                return ByteOrder(value);
            }
            return value;
        }

        public UInt32 ReadLong()
        {
            var value = reader.ReadUInt32();
            return ByteOrder(value);
        }

        public UInt16 ReadLength()
        {
            return ByteOrder(reader.ReadUInt16());
        }

        public int StreamPosition
        {
            get
            {
                return (int)reader.BaseStream.Position;
            }
        }

        static Authority[] ReadAuthority(int count)
        {
            return new Authority[0];
        }

        static Additional[] ReadAdditional(int count)
        {
            return new Additional[0];
        }

        public string ReadName()
        {
            return expander.ReadName(reader, Encoding.UTF8);
        }

        public IPAddress ReadIP6Address()
        {
            var data = reader.ReadBytes(16);
            return new IPAddress(data);
        }

        public IPAddress ReadIP4Address()
        {
            var data = reader.ReadBytes(4);
            return new IPAddress(data);
        }


        class NameExpander
        {
            Dictionary<UInt16, string> ReadStrings = new Dictionary<UInt16, string>();

            public NameExpander()
            {

            }

            public string ReadName(BinaryReader reader, Encoding encoding)
            {
                byte len = reader.ReadByte();
                if (len == 0)
                {
                    return "";
                }

                StringBuilder buf = new StringBuilder();
                if ((len & 0xc0) != 0xc0)
                {
                    var dataKey = (UInt16)(reader.BaseStream.Position - 1);
                    var data = reader.ReadBytes(len);
                    buf.Append(encoding.GetString(data));
                    var nextPart = ReadName(reader, encoding);
                    if (!string.IsNullOrEmpty(nextPart))
                    {
                        buf.Append(".");
                        buf.Append(nextPart);
                    }
                    var ret = buf.ToString();
                    ReadStrings[dataKey] = ret;
                    return ret;
                }

                UInt16 offset = (UInt16)(((len & 0x3f) << 8) | reader.ReadByte());
                return ReadStrings[offset];
            }
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


    }
}
