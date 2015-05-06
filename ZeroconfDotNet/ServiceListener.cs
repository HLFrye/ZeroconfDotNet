using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroconfDotNet
{
    public delegate IEnumerable<ServiceInfo> FindServicesDelegate(string host);

    class ServiceListener : IDisposable, ZeroconfDotNet.IServiceListener
    {
        public event FindServicesDelegate FindServices;

        public void Start()
        {

        }

        public void Stop()
        {

        }

        public void Dispose()
        {

        }

        void ServiceProcess()
        {

        }

        byte[] GenerateTxtRecord(ServiceInfo info)
        {
            return info.ToTxtRecord();
        }
    }
}
