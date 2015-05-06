using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroconfDotNet.DNS.Exceptions;

namespace ZeroconfDotNet.DNS.Records
{  
    public class PTRAnswer : AnswerData
    {
        public const int RecordType = 12;

        public string DomainName { get; set; }

        public override string ToString()
        {
            return string.Format("PTR Answer: {0}", DomainName);
        }

        public override void Read(PacketReader reader, int len)
        {
            DomainName = reader.ReadName();
        }

        public override void Write(PacketWriter writer)
        {
            writer.WriteName(DomainName);
        }


        public static PTRAnswer Deserialize(PacketReader reader)
        {
            var startPos = reader.StreamPosition;
            var len = reader.ReadLength();
            var name = reader.ReadName();
            var readBytes = reader.StreamPosition - startPos;
            if (readBytes != len)
            {
                throw new RecordLengthException(len, readBytes);
            }
            return new PTRAnswer { DomainName = name };
        }

        public static Answer Build(string name, string domainName, int TTL, bool cacheFlush, int cls)
        {
            var ans = new Answer();
            ans.Record = new Record(name, RecordType, cls);
            ans.CacheFlush = cacheFlush;
            ans.TTL = (uint)TTL;
            var ptr = new PTRAnswer();
            ptr.DomainName = domainName;
            ans.Data = ptr;
            return ans;
        }

        //public static PTRAnswer Deserialize(string name, UInt16 type, UInt16 cls, UInt32 TTL, UInt16 len, BinaryReader reader, NameExpander expander)
        //{
        //    return new PTRAnswer
        //    {
        //        Name = name,
        //        RecordType = type,
        //        Class = cls,
        //        TTL = TTL,
        //        DataLength = len,
        //        DomainName = new DNSName(expander.ReadNamePart(reader, Encoding.UTF8).Parts),
        //    };
        //}
    }
}
