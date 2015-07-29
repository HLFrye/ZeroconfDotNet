using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Net;
using System.Net.NetworkInformation;

using ZeroconfDotNet;

namespace ZeroconfPublisher
{
    class NetworkTabVM
    {
        public NetworkTabVM(NetworkInterface iface, string name)
        {
            TabCaption = name;
            Description = iface.Name;
            IP4Address = iface.GetIPProperties().UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select(x => x.Address.ToString()).First();
            IP6Address = iface.GetIPProperties().UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6).Select(x => x.Address.ToString()).First();

            PublishedServices = new ObservableCollection<PublishedService>();
        }

        public void AddService(string name, ServiceInfo info)
        {
            PublishedServices.Add(new PublishedService() { Name = name });
        }

        public string TabCaption { get; set; }

        private FrameworkElement _panel;
        public FrameworkElement TabContent
        {
            get
            {
                return _panel ?? (_panel = new PublishControl() { DataContext = this });
            }
        }

        public string Description { get; set; }
        public string IP4Address { get; set; }
        public string IP6Address { get; set; }
        public ObservableCollection<PublishedService> PublishedServices { get; set; }

    }

    class PublishedService
    {
        public string Name { get; set; }
    }
}
