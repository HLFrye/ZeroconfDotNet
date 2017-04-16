using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoveryDotNet.DNS
{
    public delegate void RequestUpdateDelegate(Tuple<string, int>[] updates);
    public interface IServiceCache
    {
        void AddPacket(Packet p);
        event RequestUpdateDelegate RequestUpdate;
    }

    public interface IServiceCache2
    {
        void WatchService(string name);
        event Action<ServiceInfo> ServiceAdded;
        event Action<ServiceInfo> ServiceExpired;
    }
}
