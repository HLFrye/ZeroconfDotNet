using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroconfDotNet.DNS;
using System.Net;
using System.Net.NetworkInformation;

namespace ZeroconfDotNet
{
    public delegate void ServiceChanged(ServiceInfo service);
    public delegate void NameChanged(string newName);

    public class ServiceListener : IDisposable, ZeroconfDotNet.IServiceListener
    {
        public static ServiceListener CreateListener(NetworkInterface nic)
        {
            var core = new ServiceCore(nic);
            core.Start();
            var timer = new ZeroconfDotNet.Utils.TimerUtil();
            var cache = new ServiceCache(timer);
            var manager = new ServiceWatchManager(core);
            var service = new ServiceListener(cache, manager, core);
            return service;
        }

        private readonly IServiceCore _core;
        private readonly IServiceCache _service;
        private readonly IServiceWatchManager _watchManager;
        private readonly Dictionary<string, IList<ServiceWatcher>> _watchers = new Dictionary<string, IList<ServiceWatcher>>();
        public ServiceListener(IServiceCache cache, IServiceWatchManager manager, IServiceCore core)
        {
            _core = core;
            _service = cache;
            _watchManager = manager;
//            _service.RequestUpdate
//            _service.ServiceAdded += _service_ServiceAdded;
//            _service.ServiceExpired += _service_ServiceExpired;
        }

        void _service_ServiceExpired(ServiceInfo obj)
        {
 	        foreach (var watcher in _watchers[obj.Name])
            {
                watcher.RemoveService(obj);
            }
        }

        void _service_ServiceAdded(ServiceInfo obj)
        {
 	        foreach (var watcher in _watchers[obj.Name])
            {
                watcher.AddService(obj);
            }
        }
        
        public ServiceWatcher FindService(string name)
        {
            var watcher = new ServiceWatcher(this, name, _watchManager);
            return watcher;            
        }
        
        internal void RemoveWatcher(ServiceWatcher watcher)
        {
            _watchers[watcher.Name].Remove(watcher);
        }

        public void Start()
        {

        }

        public void Stop()
        {

        }

        public void Dispose()
        {
            _core.Stop();
        }
    }


    public class ServiceWatcher : IDisposable
    {
        public string Name { get; private set; }
        public IEnumerable<ServiceInfo> Services;
        public ServiceChanged ServiceAdded = delegate { };
        public ServiceChanged ServiceExpired = delegate { };
        private readonly ServiceListener _listener;

        public void Start()
        {
        }

        internal ServiceWatcher(ServiceListener listener, string name, IServiceWatchManager manager)
        {
            manager.WatchService(name, (net, x) => ServiceAdded(x));

            _listener = listener;
            Name = name;
        }

        internal void AddService(ServiceInfo service)
        {
            ServiceAdded(service);
        }

        internal void RemoveService(ServiceInfo service)
        {
            ServiceExpired(service);
        }

        public void Dispose()
        {
            _listener.RemoveWatcher(this);
        }
    }
}
