using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using ZeroconfDotNet.DNS;

namespace ZeroconfDotNet
{
    class Factory
    {
        public static Factory AllNetworks = new Factory();
        public static Factory ForNetwork(NetworkInterface nic)
        {
            return new Factory(nic);
        }

        private NetworkInterface _nic = null;

        private Factory()
        {
        }

        private Factory(NetworkInterface nic)
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
