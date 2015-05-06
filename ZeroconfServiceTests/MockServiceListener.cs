using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroconfDotNet;

namespace ZeroconfServiceTests
{
    class MockServiceListener : IServiceListener
    {

        public void Dispose()
        {
            
        }

        public event FindServicesDelegate FindServices;

        public void Start()
        {
            if (FindServices == null)
            {
                throw new Exception("FindServices event must be registered before starting");
            }
        }

        public void Stop()
        {
            
        }

        public IEnumerable<ServiceInfo> RaiseFindServicesEvent(string host)
        {
            if (FindServices != null)
            {
                return FindServices(host);
            }
            throw new Exception("FindServices event was not registered");
        }        
    }
}
