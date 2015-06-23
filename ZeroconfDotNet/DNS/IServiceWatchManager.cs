using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroconfDotNet.DNS
{
    public interface IServiceWatchManager
    {
        void WatchService(string serviceName, Action<ServiceInfo> added);
    }
}
