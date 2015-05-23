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
        void WatchService(string serviceName);
        event Action<ServiceInfo> ServiceAdded;
    }

    class ThingHelper<T>
    {
        public ThingHelper(IServiceCore core)
        {
        }

        public event Action<string> Expired;

        public void Add(string name, T toAdd)
        {
        }

        public IEnumerable<T> GetAll(string name)
        {
            return Enumerable.Empty<T>();
        }

        public void Set(string name, T toSet)
        {

        }

        public T Get(string name)
        {
            return default(T);
        }
    }

    class Thing : IThing
    {
        private readonly IServiceCore _service;
        private readonly Dictionary<string, object> _watched = new Dictionary<string, object>();
        private readonly Dictionary<string, StoredAddress> _ip4Addresses = new Dictionary<string, StoredAddress>();
        private readonly Dictionary<string, StoredAddress> _ip6Addresses = new Dictionary<string, StoredAddress>();
        private readonly Dictionary<string, ServiceData> _serviceData = new Dictionary<string, ServiceData>();
        private readonly Dictionary<string, ServiceConnection> _serviceConnections = new Dictionary<string,ServiceConnection>()
        private readonly Dictionary<string, IList<ServicePointer>> _servicePointer = new Dictionary<string,IList<ServicePointer>>();

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
                    _servicePointer[ptr.Record.Name] = new List<ServicePointer>();
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
                    Address = ((AAnswer)(ip6Answer.Data)).Address,
                };
            };
        }

        public void WatchService(string serviceName)
        {
            if (WatchingService(serviceName))
            {
                return;
            }

            SendRequest(serviceName);
        }

        private bool WatchingService(string name)
        {
            return _watched.ContainsKey(name);
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

        public event Action<ServiceInfo> ServiceAdded;

        class WatchedService
        {

        }

        class StoredAddress
        {
            public uint TTL { get; set; }
            public IPAddress Address { get; set; }
        }

        class ServicePointer
        {
            public uint TTL {get; set;}
            public string ServiceName {get; set;}
        }

        class ServiceData
        {
            public uint TTL {get; set;}
            public IList<string> Flags {get; set;}
            public IDictionary<string, string> Data {get; set;}
        }

        class ServiceConnection
        {
            public uint TTL {get; set;}
            public ushort Port {get; set;}
            public uint Weight { get; set;}
            public uint Priority {get; set;}
            public string Target {get; set;}
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

            thing.WatchService(testServiceLookup);

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
            thing.ServiceAdded += info => 
            {
                serviceInfo = info;
            };
            var testPacket = BuildResponsePacket();

            thing.WatchService("scooby.doo.local");
            mockCore.Raise(x => x.PacketReceived += null, testPacket);

            Assert.AreEqual("Pubtest", serviceInfo.Name);
            Assert.AreEqual(9999, serviceInfo.Port);
            Assert.AreEqual(0, serviceInfo.Weight);
            Assert.AreEqual(0, serviceInfo.Priority);
            Assert.AreEqual("192.168.1.1", serviceInfo.IP4Address);
            Assert.AreEqual("fe80::20c:29ff:fe0d:e789", serviceInfo.IP6Address);
        }

        public Packet BuildResponsePacket()
        {
            var response = new Packet();
            response.IsQuery = false;
            response.Answers.Add(new Answer()
            {
                CacheFlush = false,
                TTL = 100,
                Record = new Record()
                {
                    Class = 1,
                    RecordType = 12,
                    Name = "scooby.doo.local",
                },
                Data = new PTRAnswer()
                {
                    DomainName = "Treats.scooby.doo.local",
                },
            });
            response.Answers.Add(new Answer()
            {
                CacheFlush = false,
                TTL = 100,
                Record = new Record()
                {
                    Class=1,
                    RecordType = 16,
                    Name="Treats.scooby.doo.local",
                },
                Data = new TXTAnswer()
                {
                }
            });
            response.Answers.Add(new Answer()
            {
                CacheFlush = false,
                TTL = 100,
                Record = new Record()
                {
                    Class = 1,
                    RecordType = 33,
                    Name = "Treats.scooby.doo.local",
                },
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
                CacheFlush = false,
                TTL = 100,
                Record = new Record()
                {
                    Class = 1,
                    RecordType = 28,
                    Name = "computer.local",
                },
                Data = new AAAAAnswer()
                {
                    Address = IPAddress.Parse("fe80::20c:29ff:fe0d:e789"),
                }
            });
            response.Answers.Add(new Answer()
            {
                CacheFlush = false,
                TTL = 100,
                Record = new Record()
                {
                    Class = 1,
                    RecordType = 1,
                    Name = "computer.local",
                },
                Data = new AAnswer()
                {
                    Address = IPAddress.Parse("192.168.1.1"),
                }
            });
            return response;
        }
    }
}
