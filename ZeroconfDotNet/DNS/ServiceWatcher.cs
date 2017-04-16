using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoveryDotNet.DNS
{

    public class ServiceWatcher
    {
        private readonly Action<ServiceInfo> _action;
        private readonly HashSet<string> _seenServices = new HashSet<string>();
        private readonly ServiceRequestRepeater _repeater;

        public ServiceWatcher(Action<ServiceInfo> action, ServiceRequestRepeater repeater)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _action = action;
            _repeater = repeater;
        }

        public void RaiseService(ServiceInfo svc)
        {
            if (_seenServices.Contains(svc.Name))
                return;

            _seenServices.Add(svc.Name);
            _action(svc);
        }

        public void Stop()
        {
            _repeater.Stop();
        }

        public void Start()
        {
            _repeater.Start();
        }
    }
}
