using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Net;
using System.Net.NetworkInformation;

using ZeroconfDotNet;

namespace ZeroconfWatcher
{
    class NetworkTab
    {
        public NetworkTab(NetworkInterface iface, string name)
        {
            TabCaption = name;
            Description = iface.Name;
            IP4Address = iface.GetIPProperties().UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select(x => x.Address.ToString()).First();
            IP6Address = iface.GetIPProperties().UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6).Select(x => x.Address.ToString()).First();

            TabContent = new NetworkTabControl() { DataContext = this };
        }

        public string TabCaption { get; set; }
        public string Description { get; set; }
        public string IP4Address { get; set; }
        public string IP6Address { get; set; }
        public FrameworkElement TabContent { get; set; }
        public ObservableCollection<SearchInfo> Searches { get; set; }

        private object _service;

        public void AddSearch(string name)
        {

        }


        public class SearchInfo
        {
            public string Protocol { get; set; }
            public ObservableCollection<ServiceInfo> Services { get; set; }
        }
    }
}
