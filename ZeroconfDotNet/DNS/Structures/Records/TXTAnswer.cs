using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoveryDotNet.DNS.Records
{
    public class TXTAnswer : AnswerData
    {
        public const int RecordType = 16;

        public TXTAnswer()
        {
            Flags = new List<string>();
            Data = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("TXT Record, {0} flags, {1} data entries\n", Flags.Count, Data.Count));
            foreach (var item in Data)
            {
                sb.Append(string.Format("{0} = {1}\n", item.Key, item.Value));
            }
            foreach (var flag in Flags)
            {
                sb.Append(flag);
                sb.Append("\n");
            }
            return sb.ToString();
        }

        public IList<string> Flags { get; set; }
        public IDictionary<string, string> Data { get; set; }

        public override void Read(PacketReader reader, int recLen)
        {
            var len = reader.ReadByte();
            var pos = reader.StreamPosition;
            while (len > (reader.StreamPosition - pos))
            {
                LoadString(reader.ReadString());
            }
            if (len != (reader.StreamPosition - pos))
            {
                throw new Exception("Read too much :(");
            }
        }

        void LoadString(string s)
        {
            if (s.Contains('='))
            {
                var parts = s.Split(new[] { '=' });
                if (!string.IsNullOrEmpty(parts[0]))
                {
                    Data[parts[0]] = parts[1];
                }
            }
            else
            {
                Flags.Add(s);
            }
        }

        public override void Write(PacketWriter writer)
        {
            foreach (var data in Data)
            {
                writer.WriteString(string.Format("{0}={1}", data.Key, data.Value));
            }
            foreach (var flag in Flags)
            {
                writer.WriteString(flag);
            }
            writer.WriteString("");            
        }

        public static Answer Build(string name, int TTL, IList<string> Flags, IDictionary<string, string> Data, bool flushCache, int cls)
        {
            var ans = new Answer();
            ans.Record = new Record(name, RecordType, cls);
            ans.CacheFlush = flushCache;
            ans.TTL = (UInt16)TTL;
            var txt = new TXTAnswer();
            txt.Flags = Flags;
            txt.Data = Data;
            ans.Data = txt;
            return ans;
        }
    }
}
