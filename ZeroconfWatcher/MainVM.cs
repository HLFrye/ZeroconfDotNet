using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;

namespace ZeroconfWatcher
{
    class MainVM
    {
        public ObservableCollection<NetworkTab> Networks { get; set; }

        public MainVM()
        {
            Networks = new ObservableCollection<NetworkTab>(EnumerateNetworks());
        }

        IEnumerable<NetworkTab> EnumerateNetworks()
        {
            //Create all-network first
            //yield return new NetworkTab("All");

            var ip4networks = NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.SupportsMulticast && !x.IsReceiveOnly && x.OperationalStatus == OperationalStatus.Up)
                .Where(x => (x.GetIPProperties().UnicastAddresses.Count > 0))
                .Where(x => x.GetIPProperties().UnicastAddresses
                                               .Select(y => y.Address)
                                               .Where(y => (y.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) || (y.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6))
                                               .Count() > 0)
                .ToList();

            for (var i = 0; i < ip4networks.Count; ++i)
            {
                var name = string.Format("Network {0}", i+1);
                yield return new NetworkTab(ip4networks[i], name);
            }

            //var ip6networks = NetworkInterface.GetAllNetworkInterfaces()
            //    .Where(x => x.SupportsMulticast && !x.IsReceiveOnly && x.OperationalStatus == OperationalStatus.Up)
            //    .SelectMany(x => x.GetIPProperties().UnicastAddresses)
            //    .Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            //    .Select(x =>
            //        new NetworkTab(x.Address.ToString())).ToList();

            //foreach (var net in ip6networks)
            //{
            //    yield return net;
            //}

        }
    }
}
