using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

using DiscoveryDotNet.DNS;
using DiscoveryDotNet.Utils;

namespace DiscoveryDotNet
{
    class MultiNetworkServicePublisher : IServicePublisher
    {
        private readonly Dictionary<NetworkInterface, ServicePublisher> _publishers = new Dictionary<NetworkInterface, ServicePublisher>();

        internal MultiNetworkServicePublisher()
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
                var publisher = new ServicePublisher(core);
                _publishers[nic] = publisher;
                publisher.NameUpdated += newName =>
                    {
                        _localName = newName;
                        DoForAll(pub =>
                        {
                            if (pub != publisher)
                            {
                                pub.LocalName = newName;
                            }
                        });
                    };
            }
        }

        
        public void AddService(string host, ServiceCallback callback)
        {
            DoForAll(x => x.AddService(host, callback));
        }

        public void AddService(string host, ServiceInfo service)
        {
            DoForAll(x => x.AddService(host, service));
        }

        public void AddService(System.Net.NetworkInformation.NetworkInterface network, string host, ServiceCallback callback)
        {
            _publishers[network].AddService(host, callback);
        }

        public void AddService(System.Net.NetworkInformation.NetworkInterface network, string host, ServiceInfo service)
        {
            _publishers[network].AddService(host, service);
        }

        public void Dispose()
        {
            DoForAll(x => x.Dispose());
        }


        private string _localName;
        public string LocalName
        {
            get
            {
                return _localName;
            }
            set
            {
                _localName = value;
                DoForAll(x => x.LocalName = value);
            }
        }

        public event Action<string> NameUpdated;

        public void Start()
        {
            DoForAll(x => x.Start());
        }

        public void Stop()
        {
            DoForAll(x => x.Stop());
        }

        void DoForAll(Action<ServicePublisher> action)
        {
            foreach (var pub in _publishers.Values)
            {
                action(pub);
            }
        }
    }
}
