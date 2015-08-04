using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

using Moq;
using ZeroconfDotNet.DNS;

namespace ZeroconfServiceTests.Utils
{
    class ConnectedServiceMock : IServiceCore
    {
        public ConnectedServiceMock(params string[] addresses)
        {
            var nic = new Mock<NetworkInterface>();
            var addrs = addresses.Select(x => IPAddress.Parse(x)).ToList();
            Network = nic.Object;
            Addresses = addrs;
        }

        public event ZeroconfDotNet.DNS.Network.NetworkStatusChangedDelegate NetworkStatusChanged;

        public bool Connected
        {
            get { return true; }
        }

        public NetworkInterface Network
        {
            get;
            private set;
        }

        public IList<IPAddress> Addresses
        {
            get;
            private set;
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
