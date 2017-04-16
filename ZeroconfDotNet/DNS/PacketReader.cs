using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using DiscoveryDotNet.DNS.Records;
using DiscoveryDotNet.DNS.Structures;
using DiscoveryDotNet.DNS.Exceptions;

namespace DiscoveryDotNet.DNS
{
    public class PacketReader
    {
        public enum PacketSectionEnum
        {
            NotStarted,
            Header,
            Questions,
            Answers,
            Authority,
            Additional
        }

        private readonly NameExpander expander;
        private BinaryReader reader;

        public PacketReader()
        {
            expander = new NameExpander();
        }

        public Packet Read(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                try
                {
                    this.reader = reader;
                    return Read();
                }
                finally
                {
                    this.reader = null;
                }
            }
        }

        public PacketSectionEnum CurrentSection { get; private set; }

        public byte[] ReadBytes(int len)
        {
            return reader.ReadBytes(len);
        }

        public Packet Read()
        {
            CurrentSection = PacketSectionEnum.Header;
            var Header = ReadHeader();

            CurrentSection = PacketSectionEnum.Questions;
            var Queries = ReadQueries(Header.Questions);

            CurrentSection = PacketSectionEnum.Answers;
            var Answers = ReadAnswers(Header.AnswerRRs);

            CurrentSection = PacketSectionEnum.Authority;
            var Authority = ReadAuthority(Header.AuthorityRRs);

            CurrentSection = PacketSectionEnum.Additional;
            var Additional = ReadAdditional(Header.AdditionalRRs);
            return new Packet(Header, Queries, Answers, Authority, Additional);
        }

        Header ReadHeader()
        {            
            return new Header
            {
                TransactionID = ByteOrder(reader.ReadUInt16()),
                Flags = ByteOrder(reader.ReadUInt16()),
                Questions = ByteOrder(reader.ReadUInt16()),
                AnswerRRs = ByteOrder(reader.ReadUInt16()),
                AuthorityRRs = ByteOrder(reader.ReadUInt16()),
                AdditionalRRs = ByteOrder(reader.ReadUInt16()),
            };            
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
            return (UInt16)IPAddress.NetworkToHostOrder((short)input);
        }

        public static UInt32 ByteOrder(UInt32 input)
        {
            return (UInt32)IPAddress.NetworkToHostOrder((int)input);
        }
    }
}
