using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroconfDotNet.DNS;

namespace ZeroconfServiceTests.Utils
{
    class ConnectedServiceMock : IServiceCore
    {
        public event ZeroconfDotNet.DNS.Network.NetworkStatusChangedDelegate NetworkStatusChanged;

        public bool Connected
        {
            get { return true; }
        }

        public ZeroconfDotNet.DNS.Network.NetworkInfo Network
        {
            get { return null; }
        }

        public void SendPacket(Packet p)
        {
            PacketReceived(p, null);
        }

        public event PacketReceivedDelegate PacketReceived;

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
