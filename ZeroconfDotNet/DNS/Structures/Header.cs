using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoveryDotNet.DNS.Structures
{
    public class Header
    {
        public UInt16 TransactionID { get; set; }
        public UInt16 Flags { get; set; }
        public UInt16 Questions { get; set; }
        public UInt16 AnswerRRs { get; set; }
        public UInt16 AuthorityRRs { get; set; }
        public UInt16 AdditionalRRs { get; set; }
    }
}
