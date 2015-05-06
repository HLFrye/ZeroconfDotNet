using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ZeroconfDotNet.DNS.Records
{
    public class AAnswer : AnswerData
    {
        public const int RecordType = 1;

        public IPAddress Address { get; set; }

        public override string ToString()
        {
            return string.Format("A Answer: {0}", Address);
        }

        public override void Read(PacketReader reader, int len)
        {
            Address = reader.ReadIP4Address();
        }

        public override void Write(PacketWriter writer)
        {
            writer.WriteIP4Address(Address);
        }

        public static Answer Build(string name, IPAddress address, int TTL, bool flushCache, UInt16 cls)
        {
            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                throw new ArgumentException("Address must be an ipv4 address");

            var ans = new Answer();
            ans.TTL = (UInt16)TTL;
            ans.Record = new Record(name, RecordType, cls);
            var data = new AAnswer();
            ans.CacheFlush = flushCache;
            data.Address = address;
            ans.Data = data;
            return ans;
        }
    }
}
