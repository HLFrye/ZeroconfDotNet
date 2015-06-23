using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ZeroconfDotNet.DNS.Network
{
    public delegate void NetworkStatusChangedDelegate(bool wasConnected, bool isConnected);

    public class NetworkInfo
    {
        public string Name { get; set; }
        public IPAddress[] Addresses { get; set; }
    }
}
