using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

using DiscoveryDotNet.DNS;

namespace DiscoveryDotNet
{
    class MultiNetworkServiceCore : IServiceCore
    {
        private readonly List<IServiceCore> _cores = new List<IServiceCore>();

        public MultiNetworkServiceCore()
        {
            var ip4networks = NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.SupportsMulticast && !x.IsReceiveOnly && x.OperationalStatus == OperationalStatus.Up)
                .Where(x => (x.GetIPProperties().UnicastAddresses.Count > 0))
                .Where(x => x.GetIPProperties().UnicastAddresses
                                               .Select(y => y.Address)
                                               .Where(y => (y.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) || (y.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6))
                                               .Count() > 0)
                .ToList();

            foreach (var nic in ip4networks)
            {
                var core = new ServiceCore(nic);
                core.PacketReceived += (p, e) =>
                {
                    PacketReceived(p, e);
                };
                core.MalformedPacketReceived += (d, r, e) =>
                {
                    MalformedPacketReceived(d, r, e);
                };
                _cores.Add(core);
            }
        }

        public event DNS.Network.NetworkStatusChangedDelegate NetworkStatusChanged;

        public bool Connected
        {
            get { return true; }
        }

        public System.Net.NetworkInformation.NetworkInterface Network
        {
            get { return null; }
        }

        public IList<System.Net.IPAddress> Addresses
        {
            get { return _cores.SelectMany(x => x.Addresses).ToList(); }
        }

        public void SendPacket(Packet p)
        {
            DoForAll(x => x.SendPacket(p));
        }

        public void SendPacket(Packet p, System.Net.IPEndPoint ep)
        {
            DoForAll(x => x.SendPacket(p, ep));
        }

        public event PacketReceivedDelegate PacketReceived = delegate { };

        public void Start()
        {
            DoForAll(x => x.Start());
        }

        public void Stop()
        {
            DoForAll(x => x.Stop());
        }

        void DoForAll(Action<IServiceCore> action)
        {
            foreach (var core in _cores)
            {
                action(core);
            }
        }



        public event MalformedPacketReceivedDelegate MalformedPacketReceived = delegate { };
    }
}
