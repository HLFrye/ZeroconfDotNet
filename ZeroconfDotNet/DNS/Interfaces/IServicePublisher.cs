using System;
using System.Net.NetworkInformation;

namespace DiscoveryDotNet
{
    public interface IServicePublisher
    {        
        void AddService(string host, ServiceCallback callback);
        void AddService(string host, ServiceInfo service);
        void AddService(NetworkInterface network, string host, ServiceCallback callback);
        void AddService(NetworkInterface network, string host, ServiceInfo service);
        void Dispose();
        string LocalName { get; set; }
        event Action<string> NameUpdated;
        void Start();
        void Stop();
    }
}
