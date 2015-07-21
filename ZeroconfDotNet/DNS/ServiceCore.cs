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
            var ip4Addr = iface.GetIPProperties().UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select(x => x.Address).First();
            _localEndpoint = new IPEndPoint(ip4Addr, 5353);

            var client = new UdpClient();
            client.Client.EnableBroadcast = true;
            client.Client.ReceiveBufferSize = 1024;
            client.Client.ExclusiveAddressUse = false;
            client.MulticastLoopback = true;
            client.Client.MulticastLoopback = true;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            client.Client.Bind(_localEndpoint);

            client.JoinMulticastGroup(IPAddress.Parse("224.0.0.251"));
            Client = client;
        }
        
        public event PacketReceivedDelegate PacketReceived;

        public void SendPacket(Packet p)
        {
            SendPacket(p, new IPEndPoint(IPAddress.Parse("224.0.0.251"), 5353));
        }

        UdpClient Client;
        bool _started = false;
        bool _stopped = false;
        IPEndPoint _localEndpoint;

        public void Start()
        {
            if (!_started)
            {
                Client.BeginReceive(new AsyncCallback(Receive), null);
                _started = true;
            }
        }

        public void Stop()
        {
            if (_started)
            {
                _stopped = true;
                Client.Close();
            }
        }

        private void Receive(IAsyncResult res)
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8000);
            if (_stopped) 
                return;
            byte[] received = Client.EndReceive(res, ref RemoteIpEndPoint);
//            if (_localEndpoint != RemoteIpEndPoint)
            {
                PacketReceived(PacketReader.Read(received), RemoteIpEndPoint);
            }
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


        public void SendPacket(Packet p, IPEndPoint ep)
        {
            var data = PacketWriter.Write(p);
            Client.Send(data, data.Length, ep);
        }
    }
}
