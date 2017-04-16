using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiscoveryDotNet.DNS.Records;
using DiscoveryDotNet.DNS.Exceptions;

namespace DiscoveryDotNet.DNS
{
    public class Answer
    {
        public UInt32 TTL { get; set; }
        public Record Record { get; set; }
        public bool CacheFlush
        {
            get
            {
                return (Record.GetRawClass() & 0x8000) != 0;
            }
            set
            {
                var rawClass = Record.GetRawClass();
                if (value)
                {
                    Record.SetRawClass((UInt16)(rawClass | 0x8000));
                }
                else
                {
                    Record.SetRawClass((UInt16)(rawClass & 0x7fff));
                }
            }
        }
        public AnswerData Data { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Answer, RecordType = {0}, TTL = {1}, Cache Flush = {2}", Record, TTL, CacheFlush);
            sb.AppendLine();
            sb.Append(Data.ToString());
            return sb.ToString();
        }

        public void ReadData(PacketReader reader)
        {
            TTL = reader.ReadLong();
            CacheFlush = (Record.GetRawClass() & 0x8000) != 0;
            var len = reader.ReadLength();
            var startPos = reader.StreamPosition; 
            CreateData(Record.RecordType);
            Data.Read(reader, len);
            var endPos = reader.StreamPosition;
            var amtRead = endPos - startPos;
            if (amtRead != len)
            {
                throw new RecordLengthException(len, amtRead);
            }
        }

        public void WriteData(PacketWriter writer)
        {
            writer.WriteShort(0);
            var sizePos = writer.GetPosition();
            Data.Write(writer);
            var dataSize = writer.GetPosition() - sizePos;
            writer.WriteAtPosition((UInt16)dataSize, sizePos - 2);
        }

        void CreateData(UInt16 dataClass)
        {
            switch (dataClass)
            {
                case 12:
                    //PTR
                    Data = new PTRAnswer();
                    return;
                case 16:
                    //TXT
                    Data = new TXTAnswer();
                    return;
                case 33:
                    //SRV
                    Data = new SRVAnswer();
                    return;
                case 28:
                    //AAAA
                    Data = new AAAAAnswer();
                    return;
                case 1:
                    //A
                    Data = new AAnswer();
                    return;
                default:
                    Data = new AnswerData();
                    return;
            }
        }
    }
}
