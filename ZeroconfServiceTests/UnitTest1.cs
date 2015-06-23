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
    [TestClass]
    public class ServiceWatchManagerTests
    {
        [TestMethod]
        public void TestWatchServiceSendsRequestPacket()
        {
            string testServiceLookup = "scooby.doo.local";
            var mockCore = new Mock<IServiceCore>();
            var thing = new ServiceWatchManager(mockCore.Object);
            Packet requestPacket = null; 

            mockCore.Setup(x => x.SendPacket(It.IsAny<Packet>())).Callback<Packet>(x => requestPacket = x).Verifiable();

            thing.WatchService(testServiceLookup, info => { });

            var ptr = requestPacket.Queries.Select(x => x.Record).Where(x => x.RecordType == 12).First();
            Assert.AreEqual(1, ptr.Class);
            Assert.AreEqual(testServiceLookup, ptr.Name);
        }

        [TestMethod]
        public void TestServiceWatchManagerCallback()
        {
            var mockCore = new Mock<IServiceCore>();
            var thing = new ServiceWatchManager(mockCore.Object);
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
