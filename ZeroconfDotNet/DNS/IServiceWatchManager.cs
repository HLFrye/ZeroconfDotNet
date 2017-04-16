using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace DiscoveryDotNet.DNS
{
    public interface IServiceWatchManager : IDisposable
    {
        ServiceWatcher WatchService(string serviceName, Action<NetworkInterface, ServiceInfo> added);
    }
}
