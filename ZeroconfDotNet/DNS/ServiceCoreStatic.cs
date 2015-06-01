using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using ZeroconfDotNet.DNS.Network;

namespace ZeroconfDotNet.DNS
{
    partial class ServiceCore
    {
        private static readonly Dictionary<NetworkInterface, ServiceCore> _services = new Dictionary<NetworkInterface, ServiceCore>();

        static ServiceCore()
        {
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
        }

        static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (_services.ContainsKey(nic))
                {
                    var service = _services[nic];
                    service.Connected = e.IsAvailable;
                }
            }
        }
    }
}
