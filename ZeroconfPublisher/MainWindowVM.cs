using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;

namespace ZeroconfPublisher
{
    class MainWindowVM
    {
        public MainWindowVM()
        {
            Networks = LoadNetworks();
        }

        IEnumerable<NetworkTabVM> EnumerateNetworks()
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
                var name = string.Format("Network {0}", i + 1);
                var tab = TryCreateTab(ip4networks[i], name);
                if (tab != null)
                    yield return tab;
            }
        }

        NetworkTabVM TryCreateTab(NetworkInterface iface, string name)
        {
            try
            {
                return new NetworkTabVM(iface, name);
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        private ObservableCollection<NetworkTabVM> LoadNetworks()
        {
            return new ObservableCollection<NetworkTabVM>(EnumerateNetworks());
        }

        public ObservableCollection<NetworkTabVM> Networks { get; set; }
    }
}
