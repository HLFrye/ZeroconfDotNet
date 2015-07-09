using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ZeroconfDotNet.DNS.Records
{
    public class AAAAAnswer : AnswerData
    {
        public const UInt16 RecordType = 28;
        public IPAddress Address { get; set; }

        public override void Read(PacketReader reader, int len)
        {
            Address = reader.ReadIP6Address();
        }

        public override string ToString()
        {
            return string.Format("AAAA Answer: {0}", Address);
        }

        public override void Write(PacketWriter writer)
        {
            writer.WriteIP6Address(Address);
        }

        public static Answer Build(string name, IPAddress address, int TTL, bool flushCache, UInt16 cls)
        {
            if (address == null)
                return null;

            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                throw new ArgumentException("Address must be an ipv6 address");

            var ans = new Answer();
            ans.TTL = (UInt16)TTL;
            ans.Record = new Record(name, RecordType, cls);
            ans.CacheFlush = flushCache;
            var data = new AAAAAnswer();
            data.Address = address;
            ans.Data = data;
            return ans;
        }
    }
}
