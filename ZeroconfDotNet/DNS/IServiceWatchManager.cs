using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace ZeroconfDotNet.DNS
{
    public interface IServiceWatchManager : IDisposable
    {
        void WatchService(string serviceName, Action<NetworkInterface, ServiceInfo> added);
        void StopWatching(string serviceName);
        void Start();
        void Stop();        
    }
}
