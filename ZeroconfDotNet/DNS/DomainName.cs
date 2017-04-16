using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ZeroconfDotNet.DNS
{
    class DomainName
    {
        public string Name { get; set; }
        public IPAddress? IP4Address { get; set; }
        public IPAddress? IP6Address { get; set; }
        public bool HasAddress { get { return IP4Address.HasValue || IP6Address.HasValue; } }
    }
}
