using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using DiscoveryDotNet.DNS;

namespace DiscoveryDotNet
{
    public class Service
    {
        public static Service AllNetworks = new Service();
        public static Service ForNetwork(NetworkInterface nic)
        {
            return new Service(nic);
        }

        private NetworkInterface _nic = null;

        private Service()
        {
        }

        private Service(NetworkInterface nic)
        {
            _nic = nic;
        }

        public IServicePublisher CreatePublisher()
        {
            if (_nic == null)
            {
                return new MultiNetworkServicePublisher();
            }
            else
            {
                var core = new ServiceCore(_nic);
                return new ServicePublisher(core);
            }
        }

        public IServiceCore GetMDNSService()
        {
            if (_nic == null)
            {
                return new MultiNetworkServiceCore();
            }
            else
            {
                return new ServiceCore(_nic);
            }
        }

        private IServiceWatchManager _watchManager;
        private readonly object _watchLock = new object();
       
        public ServiceWatcher FindService(string serviceName, Action<NetworkInterface, ServiceInfo> callback)
        {
            if (_watchManager == null)
            {
                lock (_watchLock)
                {
                    if (_watchManager == null)
                    {
                        if (_nic == null)
                        {
                            _watchManager = new MultiNetworkServiceWatcher();
                        }
                        else
                        {
                            var core = new ServiceCore(_nic);
                            _watchManager = new ServiceWatchManager(core);
                        }
                    }
                }
            }
            return _watchManager.WatchService(serviceName, callback);
        }
    }
}
