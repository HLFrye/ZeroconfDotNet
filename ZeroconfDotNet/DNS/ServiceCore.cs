using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace ZeroconfDotNet.DNS
{
    public delegate void PacketReceivedDelegate(Packet p, IPEndPoint endPoint);

    public partial class ServiceCore : ZeroconfDotNet.DNS.IServiceCore
    {
        private readonly NetworkInterface _network;
        private bool _connected;
        public bool Connected
        {
            get
            {
                return _connected;
            }

            private set
            {
                if (_connected != value)
                {
                    RaiseNetworkStatusChanged(_connected, value);
                    _connected = value;
                }
            }
        }

        public ServiceCore()
        {
            _network = null;

            var client = new UdpClient();
            client.Client.ExclusiveAddressUse = false;
            client.Client.Bind(new IPEndPoint(IPAddress.Parse("192.168.16.1"), 5353));

            client.JoinMulticastGroup(IPAddress.Parse("224.0.0.251"));
            Client = client;
        }

        public ServiceCore(NetworkInterface iface)
        {
            _network = iface;

            var client = new UdpClient();
            client.Client.ExclusiveAddressUse = false;
            client.Client.Bind(new IPEndPoint(IPAddress.Parse("192.168.16.1"), 5353));

            client.JoinMulticastGroup(IPAddress.Parse("224.0.0.251"));
            Client = client;
        }
        
        public event PacketReceivedDelegate PacketReceived;

        public void SendPacket(Packet p)
        {
            var data = PacketWriter.Write(p);
            Client.Send(data, data.Length, new IPEndPoint(IPAddress.Parse("224.0.0.251"), 5353));
        }

        UdpClient Client;
        bool _started = false;

        public void Start()
        {
            if (!_started)
                Client.BeginReceive(new AsyncCallback(Receive), null);        
        }

        private void Receive(IAsyncResult res)
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8000);
            byte[] received = Client.EndReceive(res, ref RemoteIpEndPoint);
            PacketReceived(PacketReader.Read(received), RemoteIpEndPoint);                        
            Client.BeginReceive(new AsyncCallback(Receive), null);
        }

        public event Network.NetworkStatusChangedDelegate NetworkStatusChanged;

        private void RaiseNetworkStatusChanged(bool was, bool isNow)
        {
            if (NetworkStatusChanged != null)
            {
                NetworkStatusChanged(was, isNow);
            }
        }

        public Network.NetworkInfo Network
        {
            get 
            {
                return new Network.NetworkInfo
                {
                    Name = _network.Name,
                    Addresses = _network.GetIPProperties().UnicastAddresses.Select(x => x.Address).ToArray(),
                };
            }
        }


        public bool Status
        {
            get 
            {
                switch (_network.OperationalStatus)
                {
                    case OperationalStatus.Up:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}
