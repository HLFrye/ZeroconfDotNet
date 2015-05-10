using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroconfDotNet
{
    delegate void ServiceChanged(ServiceInfo service);
    class ServiceWatcher
    {
        public IEnumerable<ServiceInfo> Services;
        public ServiceChanged ServiceAdded = delegate { };
        public ServiceChanged ServiceExpired = delegate { };
        public void Refresh()
        {

        }
    }

    class ServiceListener : IDisposable, ZeroconfDotNet.IServiceListener
    {
        public ServiceWatcher FindService(string name)
        {
            return null;
        }

        public void Start()
        {

        }

        public void Stop()
        {

        }

        public void Dispose()
        {

        }

        public event FindServicesDelegate FindServices;
    }
}
