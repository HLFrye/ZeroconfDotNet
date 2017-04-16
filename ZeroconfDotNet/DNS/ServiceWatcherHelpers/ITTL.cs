using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoveryDotNet.DNS
{
    public partial class ServiceWatchManager
    {
        interface ITTL
        {
            DateTime ExpireAt { get; }
        }
    }
}
