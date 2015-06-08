using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroconfDotNet.DNS;

namespace ZeroconfDotNet
{
    public delegate void ServiceChanged(ServiceInfo service);
    public delegate void NameChanged(string newName);

    class ServiceListener : IDisposable, ZeroconfDotNet.IServiceListener
    {
        private readonly IServiceCache2 _service;
        private readonly Dictionary<string, IList<ServiceWatcher>> _watchers = new Dictionary<string, IList<ServiceWatcher>>();
        public ServiceListener(IServiceCache2 cache)
        {
            _service = cache;
            _service.ServiceAdded += _service_ServiceAdded;
            _service.ServiceExpired += _service_ServiceExpired;
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

        void _service_PacketReceived(Packet p, System.Net.IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public ServiceWatcher FindService(string name)
        {
            throw new NotImplementedException();
            var watcher = new ServiceWatcher(this, name);
            if (_watchers.ContainsKey(name))
            {
                _watchers[name] = new List<ServiceWatcher>();
            }
            _watchers[name].Add(watcher);
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

        internal ServiceWatcher(ServiceListener listener, string name)
        {
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
