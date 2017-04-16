using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoveryDotNet.DNS
{
    public class Query
    {
        public bool IsMulticast { get; set; }
        public Record Record { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(IsMulticast ? "Multicast query" : "Unicast query");
            sb.AppendLine();
            sb.Append(Record.ToString());
            return sb.ToString();
        }
    }
}
