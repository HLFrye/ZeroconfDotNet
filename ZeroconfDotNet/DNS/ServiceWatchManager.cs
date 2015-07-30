using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

using ZeroconfDotNet.DNS.Records;

namespace ZeroconfDotNet.DNS
{
    public partial class ServiceWatchManager : IServiceWatchManager
    {
        private readonly IServiceCore _service;
        private readonly Dictionary<string, IList<ServiceWatcher>> _watched = new Dictionary<string, IList<ServiceWatcher>>();
        private readonly TTLDict<StoredAddress> _ip4Addresses = new TTLDict<StoredAddress>();
        private readonly TTLDict<StoredAddress> _ip6Addresses = new TTLDict<StoredAddress>();
        private readonly TTLDict<ServiceData> _serviceData = new TTLDict<ServiceData>();
        private readonly TTLDict<ServiceConnection> _serviceConnections = new TTLDict<ServiceConnection>();
        private readonly Dictionary<string, TTLList<ServicePointer>> _servicePointer = new Dictionary<string, TTLList<ServicePointer>>();
        private readonly NetworkInterface _nic;
        public ServiceWatchManager(IServiceCore core, NetworkInterface nic)
        {
            _nic = nic;
            _service = core;
            _service.PacketReceived += _service_PacketReceived;
            _service.Start();
        }

        void _service_PacketReceived(Packet p, IPEndPoint endPoint)
        {
            HandleAddressRecords(p);
            HandleServiceInfoRecords(p);
            HandlePTRResponse(p);

            RaiseServiceEvents();
        }

        void RaiseServiceEvents()
        {
            IEnumerable<ServiceInfo> _activeServices = GetActiveServices();
            foreach (var info in _activeServices)
            {
                if (_watched.ContainsKey(info.Protocol))
                {
                    foreach (var watcher in _watched[info.Protocol])
                    {
                        watcher.RaiseService(info);
                    }
                }
            }
        }

        IEnumerable<ServiceInfo> GetActiveServices()
        {
            foreach (var data in _servicePointer)
            {
                foreach (var ptr in data.Value)
                {
                    var info = new ServiceInfo();
                    var svcName = ptr.ServiceName;
                    info.Name = svcName.Substring(0, svcName.IndexOf('.'));
                    info.Protocol = svcName.Substring(svcName.IndexOf('.') + 1);

                    bool complete = true;
                    if (_serviceData.ContainsKey(svcName))
                    {
                        var txt = _serviceData[svcName];
                        info.Flags = txt.Flags;
                        info.Data = txt.Data;
                    }
                    else
                    {
                        RequestServiceData(svcName);
                        complete = false;
                    }

                    string svcTarget = null;
                    if (_serviceConnections.ContainsKey(svcName))
                    {

                        var srv = _serviceConnections[svcName];
                        info.Port = srv.Port;
                        info.Weight = (int)srv.Weight;
                        info.Priority = (int)srv.Priority;
                        svcTarget = srv.Target;
                    }
                    else
                    {
                        RequestServiceConnection(svcName);
                        complete = false;
                    }

                    if (svcTarget != null)
                    {
                        StoredAddress ip4Address;
                        if (_ip4Addresses.TryGetValue(svcTarget, out ip4Address))
                        {
                            info.IP4Address = ip4Address.Address;
                        }

                        StoredAddress ip6Address;
                        if (_ip6Addresses.TryGetValue(svcTarget, out ip6Address))
                        {
                            info.IP6Address = ip6Address.Address;
                        }
                        else
                        {
                            if (ip4Address == null)
                            {
                                RequestAddress(svcTarget);
                                complete = false;
                            }
                        }
                    }

                    if (complete)
                        yield return info;
                }
            }
        }

        void AddToPacket(ref Packet p, string name, int requestType)
        {
            if (p == null)
            {
                p = new Packet();
                p.IsQuery = true;
            }
            p.Queries.Add(new Query()
            {
                IsMulticast = false,
                Record = new Record()
                {
                    Name = name,
                    RecordType = (ushort)requestType,
                    Class = 1,
                }
            });
        }

        void RequestAddress(string name)
        {
            Packet packet = null;
            AddToPacket(ref packet, name, 1);
            AddToPacket(ref packet, name, 28);
            _service.SendPacket(packet);
        }

        void RequestServiceConnection(string name)
        {
            Packet packet = null;
            AddToPacket(ref packet, name, 33);
            _service.SendPacket(packet);
        }

        void RequestServiceData(string name)
        {
            Packet packet = null;
            AddToPacket(ref packet, name, 16);
            _service.SendPacket(packet);
        }

        void HandleServiceInfoRecords(Packet p)
        {
            foreach (var txt in p.Answers.Where(x => x.Record.RecordType == 16))
            {
                var answerData = txt.Data as TXTAnswer;
                var data = new ServiceData();
                data.TTL = txt.TTL;
                data.Flags = answerData.Flags;
                data.Data = answerData.Data;
                _serviceData[txt.Record.Name] = data;
            }

            foreach (var srv in p.Answers.Where(x => x.Record.RecordType == 33))
            {
                var srvData = srv.Data as SRVAnswer;
                var data = new ServiceConnection();
                data.TTL = srv.TTL;
                data.Port = srvData.Port;
                data.Priority = srvData.Priority;
                data.Target = srvData.Target;
                data.Weight = srvData.Weight;
                _serviceConnections[srv.Record.Name] = data;
            }
        }

        void HandlePTRResponse(Packet p)
        {
            foreach (var ptr in p.Answers.Where(x => x.Record.RecordType == 12))
            {
                var ptrData = ptr.Data as PTRAnswer;
                var pointer = new ServicePointer()
                {
                    TTL = ptr.TTL,
                    ServiceName = ptrData.DomainName,
                };
                if (!_servicePointer.ContainsKey(ptr.Record.Name))
                {
                    _servicePointer[ptr.Record.Name] = new TTLList<ServicePointer>();
                }
                _servicePointer[ptr.Record.Name].Add(pointer);
            }
        }

        void HandleAddressRecords(Packet p)
        {
            foreach (var ip4Answer in p.Answers.Where(x => x.Record.RecordType == 1))
            {
                _ip4Addresses[ip4Answer.Record.Name] = new StoredAddress
                {
                    TTL = ip4Answer.TTL,
                    Address = ((AAnswer)(ip4Answer.Data)).Address,
                };
            }

            foreach (var ip6Answer in p.Answers.Where(x => x.Record.RecordType == 28))
            {
                _ip6Addresses[ip6Answer.Record.Name] = new StoredAddress
                {
                    TTL = ip6Answer.TTL,
                    Address = ((AAAAAnswer)(ip6Answer.Data)).Address,
                };
            };
        }

        public void WatchService(string serviceName, Action<NetworkInterface, ServiceInfo> added)
        {
            if (!_watched.ContainsKey(serviceName))
            {
                _watched[serviceName] = new List<ServiceWatcher>();
            }
            _watched[serviceName].Add(new ServiceWatcher(x => added(_nic, x));

            var repeater = new ServiceRequestRepeater(_service, serviceName, new Utils.TimerUtil());

            //SendRequest(serviceName);
        }
        
        class StoredAddress : ITTL
        {
            private uint _ttl;
            public uint TTL
            {
                get
                {
                    return _ttl;
                }
                set
                {
                    _ttl = value;
                    ExpireAt = DateTime.Now + TimeSpan.FromSeconds(_ttl);
                }
            }

            public IPAddress Address { get; set; }

            public DateTime ExpireAt
            {
                get;
                private set;
            }
        }

        class ServicePointer : ITTL
        {
            private uint _ttl;
            public uint TTL
            {
                get
                {
                    return _ttl;
                }
                set
                {
                    _ttl = value;
                    ExpireAt = DateTime.Now + TimeSpan.FromSeconds(_ttl);
                }
            }

            public string ServiceName { get; set; }

            public DateTime ExpireAt
            {
                get;
                private set;
            }
        }

        class ServiceData : ITTL
        {
            private uint _ttl;
            public uint TTL
            {
                get
                {
                    return _ttl;
                }
                set
                {
                    _ttl = value;
                    ExpireAt = DateTime.Now + TimeSpan.FromSeconds(_ttl);
                }
            }

            public IList<string> Flags { get; set; }
            public IDictionary<string, string> Data { get; set; }

            public DateTime ExpireAt
            {
                get;
                private set;
            }
        }

        class ServiceConnection : ITTL
        {
            private uint _ttl;
            public uint TTL
            {
                get
                {
                    return _ttl;
                }
                set
                {
                    _ttl = value;
                    ExpireAt = DateTime.Now + TimeSpan.FromSeconds(_ttl);
                }
            }

            public ushort Port { get; set; }
            public uint Weight { get; set; }
            public uint Priority { get; set; }
            public string Target { get; set; }

            public DateTime ExpireAt
            {
                get;
                private set;
            }
        }

        class ServiceWatcher
        {
            private readonly Action<ServiceInfo> _action;
            private readonly HashSet<string> _seenServices = new HashSet<string>();

            public ServiceWatcher(Action<ServiceInfo> action)
            {
                if (action == null)
                    throw new ArgumentNullException("action");

                _action = action;
            }

            public void RaiseService(ServiceInfo svc)
            {
                if (_seenServices.Contains(svc.Name))
                    return;

                _seenServices.Add(svc.Name);
                _action(svc);
            }
        }

        public void StopWatching(string serviceName)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
