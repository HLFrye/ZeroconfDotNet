using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;

using ZeroconfDotNet;
using ZeroconfDotNet.DNS;
using ZeroconfDotNet.DNS.Records;

namespace ZeroconfServiceTests
{
    interface IThing
    {
        void WatchService(string serviceName, Action<ServiceInfo> added);        
    }

    interface ITTL
    {
        DateTime ExpireAt {get;}
    }

    class TTLList<T> : IList<T> where T : ITTL
    {
        public TTLList()
            :this(new List<T>())
        {
        }

        private readonly IList<T> _base;
        public TTLList(IList<T> _baseList)
        {
            _base = _baseList;
        }

        void Cleanup()
        {
            var toRemove = _base.Where(x => x.ExpireAt < DateTime.Now).ToList();
            foreach (var item in toRemove)
            {
                _base.Remove(item);
            }
        }

        public int IndexOf(T item)
        {
            Cleanup();
            return _base.IndexOf(item);            
        }

        public void Insert(int index, T item)
        {
            _base.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _base.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                Cleanup();
                return _base[index];
            }
            set
            {
                _base[index] = value;
            }
        }

        public void Add(T item)
        {
            _base.Add(item);
        }

        public void Clear()
        {
            _base.Clear();
        }

        public bool Contains(T item)
        {
            Cleanup();
            return _base.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Cleanup();
            _base.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                Cleanup();
                return _base.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return _base.IsReadOnly; }
        }

        public bool Remove(T item)
        {
            return _base.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            Cleanup();
            return _base.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            Cleanup();
            return _base.GetEnumerator();
        }
    }

    class TTLDict<T> : IDictionary<string, T> where T : ITTL
    {
        private readonly IDictionary<string, T> _base;
        public TTLDict()
            : this(new Dictionary<string, T>())
        {}

        public TTLDict(IDictionary<string, T> baseDict)
        {
            _base = baseDict;
        }

        DateTime _lastClean = DateTime.Now;
        TimeSpan refreshTime = TimeSpan.FromSeconds(5);
        void Cleanup()
        {
            if (DateTime.Now - _lastClean < refreshTime)
                return;
            _lastClean = DateTime.Now;

            var toRemove = _base.Where(data => data.Value.ExpireAt < DateTime.Now).Select(data => data.Key).ToList();
            foreach (var key in toRemove)
            {
                _base.Remove(key);
            }
        }

        public void Add(string key, T value)
        {
 	        _base.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            Cleanup();
 	        return _base.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
	        get 
            { 
                Cleanup();
                return _base.Keys;
            }
        }

        public bool Remove(string key)
        {
 	        return _base.Remove(key);
        }

        public bool TryGetValue(string key, out T value)
        {
 	        Cleanup();
            return _base.TryGetValue(key, out value);
        }

        public ICollection<T> Values
        {
	        get { Cleanup(); return _base.Values; }
        }

        public T this[string key]
        {
	        get 
	        { 
                  Cleanup();
                  return _base[key];
	        }
	          set 
	        { 
		        _base[key] = value;
	        }
        }

        public void Add(KeyValuePair<string,T> item)
        {
 	        _base.Add(item);
        }

        public void Clear()
        {
 	        _base.Clear();
        }

        public bool Contains(KeyValuePair<string,T> item)
        {
            Cleanup();
            return _base.Contains(item);
 	
        }

        public void CopyTo(KeyValuePair<string,T>[] array, int arrayIndex)
        {
 	        Cleanup();
            _base.CopyTo(array, arrayIndex);
        }

        public int Count
        {
	        get { Cleanup(); return _base.Count; }
        }

        public bool IsReadOnly
        {
	        get { return _base.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string,T> item)
        {
 	        return _base.Remove(item);
        }

        public IEnumerator<KeyValuePair<string,T>> GetEnumerator()
        {
 	        Cleanup();
            return _base.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
 	        Cleanup();
            return _base.GetEnumerator();
        }
    }

    class Thing : IThing
    {
        private readonly IServiceCore _service;
        private readonly Dictionary<string, IList<ServiceWatcher>> _watched = new Dictionary<string, IList<ServiceWatcher>>();
        private readonly TTLDict<StoredAddress> _ip4Addresses = new TTLDict<StoredAddress>();
        private readonly TTLDict<StoredAddress> _ip6Addresses = new TTLDict<StoredAddress>();
        private readonly TTLDict<ServiceData> _serviceData = new TTLDict<ServiceData>();
        private readonly TTLDict<ServiceConnection> _serviceConnections = new TTLDict<ServiceConnection>();
        private readonly Dictionary<string, TTLList<ServicePointer>> _servicePointer = new Dictionary<string, TTLList<ServicePointer>>();

        public Thing(IServiceCore core)
        {
            _service = core;
            _service.PacketReceived += _service_PacketReceived;
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
                foreach (var watcher in _watched[info.Protocol])
                {
                    watcher.RaiseService(info);
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
                IsMulticast = true,
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
                var answerData  = txt.Data as TXTAnswer;
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

        public void WatchService(string serviceName, Action<ServiceInfo> added)
        {
            if (!_watched.ContainsKey(serviceName))
            {
                _watched[serviceName] = new List<ServiceWatcher>();
            }
            _watched[serviceName].Add(new ServiceWatcher(added));
            
            SendRequest(serviceName);
        }

        private void SendRequest(string name)
        {
            var packet = new Packet();
            packet.IsQuery = true;
            packet.Queries.Add(new Query()
            {
                IsMulticast = true,
                Record = new Record()
                {
                    Class = 1,
                    Name = name,
                    RecordType = 12,
                }
            });

            _service.SendPacket(packet);
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

            public string ServiceName {get; set;}

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

            public IList<string> Flags {get; set;}
            public IDictionary<string, string> Data {get; set;}

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

            public ushort Port {get; set;}
            public uint Weight { get; set;}
            public uint Priority {get; set;}
            public string Target {get; set;}

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
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod2()
        {
            string testServiceLookup = "scooby.doo.local";
            var mockCore = new Mock<IServiceCore>();
            var thing = new Thing(mockCore.Object);
            Packet requestPacket = null; ;

            mockCore.Setup(x => x.SendPacket(It.IsAny<Packet>())).Callback<Packet>(x => requestPacket = x).Verifiable();

            thing.WatchService(testServiceLookup, info => { });

            var ptr = requestPacket.Queries.Select(x => x.Record).Where(x => x.RecordType == 12).First();
            Assert.AreEqual(1, ptr.Class);
            Assert.AreEqual(testServiceLookup, ptr.Name);
        }

        [TestMethod]
        public void TestMethod1()
        {
            var mockCore = new Mock<IServiceCore>();
            var thing = new Thing(mockCore.Object);
            ServiceInfo serviceInfo = null;

            thing.WatchService("scooby.doo.local",  info => 
            {
                serviceInfo = info;
            });
            var testPacket = BuildResponsePacket();

            mockCore.Raise(x => x.PacketReceived += null, testPacket, new IPEndPoint(IPAddress.Parse("192.168.1.1"), 5353));

            Assert.AreEqual("Treats", serviceInfo.Name);
            Assert.AreEqual("scooby.doo.local", serviceInfo.Protocol);
            Assert.AreEqual(9999, serviceInfo.Port);
            Assert.AreEqual(0, serviceInfo.Weight);
            Assert.AreEqual(0, serviceInfo.Priority);
            Assert.AreEqual(IPAddress.Parse("192.168.1.1"), serviceInfo.IP4Address);
            Assert.AreEqual(IPAddress.Parse("fe80::20c:29ff:fe0d:e789"), serviceInfo.IP6Address);
        }

        public Packet BuildResponsePacket()
        {
            var response = new Packet();
            response.IsQuery = false;
            response.Answers.Add(new Answer()
            {
                Record = new Record()
                {
                    Class = 1,
                    RecordType = 12,
                    Name = "scooby.doo.local",
                },
                CacheFlush = false,
                TTL = 100,
                Data = new PTRAnswer()
                {
                    DomainName = "Treats.scooby.doo.local",
                },
            });
            response.Answers.Add(new Answer()
            {
                Record = new Record()
                {
                    Class = 1,
                    RecordType = 16,
                    Name = "Treats.scooby.doo.local",
                },
                CacheFlush = false,
                TTL = 100,
                Data = new TXTAnswer()
                {
                }
            });
            response.Answers.Add(new Answer()
            {
                Record = new Record()
                {
                    Class = 1,
                    RecordType = 33,
                    Name = "Treats.scooby.doo.local",
                },
                CacheFlush = false,
                TTL = 100,
                Data = new SRVAnswer()
                {
                    Name = "computer.local",
                    Port = 9999,
                    Weight = 0,
                    Priority = 0,
                    Target = "computer.local",
                }
            });
            response.Answers.Add(new Answer()
            {
                Record = new Record()
                {
                    Class = 1,
                    RecordType = 28,
                    Name = "computer.local",
                },
                CacheFlush = false,
                TTL = 100,
                Data = new AAAAAnswer()
                {
                    Address = IPAddress.Parse("fe80::20c:29ff:fe0d:e789"),
                }
            });
            response.Answers.Add(new Answer()
            {
                Record = new Record()
                {
                    Class = 1,
                    RecordType = 1,
                    Name = "computer.local",
                },
                CacheFlush = false,
                TTL = 100,
                Data = new AAnswer()
                {
                    Address = IPAddress.Parse("192.168.1.1"),
                }
            });
            return response;
        }
    }
}
