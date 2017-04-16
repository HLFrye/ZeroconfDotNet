using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoveryDotNet.DNS.Records
{
    public class SRVAnswer :AnswerData
    {
        public const int RecordType = 33;
        public UInt16 Priority { get; set; }
        public UInt16 Weight { get; set; }
        public UInt16 Port { get; set; }

        public override string ToString()
        {
            return string.Format("SRV Answer: Priority {0}, Weight {1}, Port {2}, Target: {3}", Priority, Weight, Port, Target);
        }

        public static string GetService(Record r)
        {
            return r.Name.Substring(0, r.Name.IndexOf('.'));
        }

        public static string GetProtocol(Record r)
        {
            var serviceEnd = r.Name.IndexOf('.') + 1;
            var protocolEnd = r.Name.IndexOf('.', serviceEnd);
            return r.Name.Substring(serviceEnd, protocolEnd - serviceEnd);            
        }

        public static string GetName(Record r)
        {
            var serviceEnd = r.Name.IndexOf('.') + 1;
            var protocolEnd = r.Name.IndexOf('.', serviceEnd) + 1;
            return r.Name.Substring(protocolEnd);
        }

        public string Name { get; set; }
        public string Target { get; set; }

        public override void Read(PacketReader reader, int len)
        {
            Priority = reader.ReadShort();
            Weight = reader.ReadShort();
            Port = reader.ReadShort();
            Target = reader.ReadName();
        }

        public override void Write(PacketWriter writer)
        {
            writer.WriteShort(Priority);
            writer.WriteShort(Weight);
            writer.WriteShort(Port);
            writer.WriteName(Target);
        }

        public static Answer Build(string domainName, int TTL, int priority, int weight, int port, string target, bool cacheFlush, int cls)
        {
            var record = new Record(domainName, RecordType, cls);
            var ans = new Answer();
            ans.Record = record;
            ans.CacheFlush = cacheFlush;
            ans.TTL = (UInt32)TTL;
            var srv = new SRVAnswer();
            srv.Priority = (UInt16)priority;
            srv.Weight = (UInt16)weight;
            srv.Port = (UInt16)port;
            srv.Target = target;
            ans.Data = srv;
            return ans;
        }
    }
}
