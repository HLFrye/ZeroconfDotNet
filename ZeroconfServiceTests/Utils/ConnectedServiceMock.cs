using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using ZeroconfDotNet.DNS;

namespace ZeroconfServiceTests.Utils
{
    class ConnectedServiceMock : IServiceCore
    {
        public ConnectedServiceMock(params string[] addresses)
        {
            Network = new ZeroconfDotNet.DNS.Network.NetworkInfo();
            Network.Addresses = addresses.Select(x => IPAddress.Parse(x)).ToArray();
        }

        public event ZeroconfDotNet.DNS.Network.NetworkStatusChangedDelegate NetworkStatusChanged;

        public bool Connected
        {
            get { return true; }
        }

        public ZeroconfDotNet.DNS.Network.NetworkInfo Network
        {
            get;
            set;
        }

        public void SendPacket(Packet p)
        {
            PacketReceived(p, null);
        }

        public event PacketReceivedDelegate PacketReceived;

        public bool Running { get; set; }

        public void Start()
        {
            Running = true;
        }

        public void Stop()
        {
            Running = false;
        }


        public void SendPacket(Packet p, System.Net.IPEndPoint ep)
        {
            PacketReceived(p, ep);
        }
    }
}
