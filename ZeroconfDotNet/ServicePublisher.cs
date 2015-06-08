using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ZeroconfDotNet.DNS;

namespace ZeroconfDotNet
{
    public delegate ServiceInfo ServiceCallback();

    /// <summary>
    /// Main entry point.  Manages a mDNS listener that will respond
    /// to queries with any services registered to this publisher
    /// </summary>
    public class ServicePublisher : IDisposable
    {
        private readonly IServiceListener _listener;
        private ILookup<string, ServiceCallback> _lookup;
        private IList<Tuple<string, ServiceCallback>> _callbacks = new List<Tuple<string, ServiceCallback>>();
        private object _lookupLock = new object();
        private bool _started = false;
        public ServicePublisher(IServiceCache2 cache)
            :this(new ServiceListener(cache))
        {
            
        }

        public ServicePublisher(IServiceListener listener)
        {
            _listener = listener;
            //_listener.FindService
            //listener.FindServices += listener_FindServices;
        }

        IEnumerable<ServiceInfo> listener_FindServices(string host)
        {
            ServiceCallback[] callbacks;
            lock (_lookupLock)
            {
                callbacks = _lookup[host].ToArray();
            }
            return callbacks.Select(x => x());
        }

        public void AddService(string host, ServiceInfo service)
        {
            _callbacks.Add(Tuple.Create<string, ServiceCallback>(host, () => service));
            CreateLookup();
        }

        private void CreateLookup()
        {
            if (_started)
            {
                Monitor.Enter(_lookupLock);
            }
            _lookup = _callbacks.ToLookup(x => x.Item1, x => x.Item2);
            if (_started)
            {
                Monitor.Exit(_lookupLock);
            }
        }

        public void AddService(string host, ServiceCallback callback)
        {
            _callbacks.Add(Tuple.Create(host, callback));
            CreateLookup();
        }

        public void Start()
        {
            _started = true;
            CreateLookup();
            _listener.Start();
        }

        public void Stop()
        {
            _listener.Stop();
            _started = false;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
