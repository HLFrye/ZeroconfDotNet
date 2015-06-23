using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroconfDotNet.DNS
{
    public partial class ServiceWatchManager
    {
        interface ITTL
        {
            DateTime ExpireAt { get; }
        }
    }
}
