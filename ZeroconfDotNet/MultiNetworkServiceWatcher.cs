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
    class MultiNetworkServiceWatcher : IServiceWatchManager
    {
        private readonly List<ServiceWatchManager> _watchers = new List<ServiceWatchManager>();

        internal MultiNetworkServiceWatcher()
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
                var watcher = new ServiceWatchManager(core);
                _watchers.Add(watcher);
            }
        }

        public ServiceWatcher WatchService(string serviceName, Action<NetworkInterface, ServiceInfo> added)
        {
            //Todo: Create aggregate ServiceWatcher
            DoForAll(x => x.WatchService(serviceName, added));
            return null;
        }

        public void Dispose()
        {
            DoForAll(x => x.Dispose());
        }

        void DoForAll(Action<ServiceWatchManager> action)
        {
            foreach (var watcher in _watchers)
            {
                action(watcher);
            }
        }

    }
}
