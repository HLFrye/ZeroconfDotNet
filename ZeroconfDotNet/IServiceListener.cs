using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ZeroconfDotNet
{
    public delegate IEnumerable<ServiceInfo> FindServicesDelegate(string service);

    public interface IServiceListener
    {
        void Dispose();
        void Start();
        void Stop();
        ServiceWatcher FindService(string name);
    }
}
