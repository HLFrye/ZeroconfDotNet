using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZeroconfDotNet;
using ZeroconfDotNet.DNS;
using ZeroconfDotNet.DNS.Records;
using ZeroconfDotNet.DNS.Network;
using System.Linq;
using Moq;

namespace ZeroconfServiceTests
{
    [TestClass]
    public class ServicePublisherTests
    {
        [TestMethod]
        public void StartingPublisherCallsStartListener()
        {
            var mockListener = new Moq.Mock<IServiceCore>();
            mockListener.Setup(x => x.Start()).Verifiable();
            using (var service = new ServicePublisher(mockListener.Object))
            {
                service.Start();
            }
            mockListener.VerifyAll();
        }

        [TestMethod]
        public void ConstructingSendsNameTest()
        {
            var core = new Mock<IServiceCore>();
            Packet received = null;
            core.Setup(x => x.SendPacket(It.IsAny<Packet>())).Callback<Packet>(x => received = x);

            using (var service = new ServicePublisher(core.Object))
            {
                service.LocalName = "Scooby";
                service.Start();
                Assert.IsNotNull(received);
                var query = received.Queries[0];
                Assert.AreEqual("Scooby.local", query.Record.Name);
                Assert.AreEqual(255, query.Record.RecordType);
            }
        }

        [TestMethod]
        public void RespondToNameRequest()
        {
            var core = BuildService("10.99.99.99");
            Packet received = null;
            core.Setup(x => x.SendPacket(It.IsAny<Packet>())).Callback<Packet>(x => received = x);

            using (var service = new ServicePublisher(core.Object))
            {
                service.LocalName = "Scooby";
                service.Start();

                var nameSearchPacket = new Packet()
                {
                    IsQuery = true,
                };
                nameSearchPacket.Queries.Add(new Query()
                {
                    IsMulticast = true,
                    Record = new Record()
                    {
                        RecordType = 255,
                        Name = "Scooby.local",
                    }
                });

                core.Raise(x => x.PacketReceived += null, nameSearchPacket, new IPEndPoint(IPAddress.Parse("10.0.0.10"), 5353));

                var aanswer = received.Answers.Where(x => x.Record.RecordType == AAnswer.RecordType).First().Data as AAnswer;
                Assert.AreEqual(IPAddress.Parse("10.99.99.99"), aanswer.Address);
            }

        }

        [TestMethod]
        public void RespondWithServiceInfo()
        {
            var core = BuildService("10.99.99.99", "fe80::20c:29ff:fe0d:e789");
            Packet received = null;
            core.Setup(x => x.SendPacket(It.IsAny<Packet>())).Callback<Packet>(x => received = x);

            using (var service = new ServicePublisher(core.Object))
            {
                var publishInfo = new ServiceInfo();
                publishInfo.Name = "Pubtest";
                publishInfo.IP4Address = IPAddress.Parse("10.0.0.10");
                publishInfo.IP6Address = IPAddress.Parse("fe80::20c:29ff:fe0d:e789");
                publishInfo.Port = 9999;
                publishInfo.Priority = 10;
                publishInfo.Weight = 10;
                publishInfo.Flags.Add("TestFlag");
                publishInfo.Data["TestData"] = "TestValue";

                service.LocalName = "Scooby";
                service.AddService("_pubtest._tcp.local", publishInfo);
                service.Start();
                core.Raise(x => x.PacketReceived += null, BuildQueryPacket("_pubtest._tcp.local"), new IPEndPoint(IPAddress.Parse("10.0.0.10"), 5353));

                Assert.IsFalse(received.IsQuery);
                var ptrAnswer = received.Answers.Where(x => x.Record.RecordType == PTRAnswer.RecordType).First().Data as PTRAnswer;
                Assert.AreEqual("Pubtest._pubtest._tcp.local", ptrAnswer.DomainName);

                var srv = received.Answers.Where(x => x.Record.RecordType == SRVAnswer.RecordType).First();
                var srvAnswer = srv.Data as SRVAnswer;
                Assert.AreEqual(SRVAnswer.GetService(srv.Record), "Pubtest");
                Assert.AreEqual(SRVAnswer.GetProtocol(srv.Record), "_pubtest");
                Assert.AreEqual(SRVAnswer.GetName(srv.Record), "_tcp.local");

                //Assert.AreEqual("Pubtest._pubtest._tcp.local", srvAnswer.Name);
                Assert.AreEqual(9999, srvAnswer.Port);
                Assert.AreEqual(10, srvAnswer.Priority);
                Assert.AreEqual(10, srvAnswer.Weight);
                Assert.AreEqual("Scooby.local", srvAnswer.Target);

                var txtAnswer = received.Answers.Where(x => x.Record.RecordType == TXTAnswer.RecordType).First().Data as TXTAnswer;
                Assert.AreEqual("TestFlag", txtAnswer.Flags[0]);
                Assert.AreEqual("TestValue", txtAnswer.Data["TestData"]);

                var aAnswer = received.Answers.Where(x => x.Record.RecordType == AAnswer.RecordType).First().Data as AAnswer;
                Assert.AreEqual(IPAddress.Parse("10.99.99.99"), aAnswer.Address);

                var aaaaAnswer = received.Answers.Where(x => x.Record.RecordType == AAAAAnswer.RecordType).First().Data as AAAAAnswer;
                Assert.AreEqual(IPAddress.Parse("fe80::20c:29ff:fe0d:e789"), aaaaAnswer.Address);
            }
        }

        Packet BuildQueryPacket(string proto)
        {
            var packet = new Packet();
            packet.IsQuery = true;
            packet.Queries.Add(new Query()
            {
                IsMulticast = true,
                Record = new Record()
                {
                    RecordType = PTRAnswer.RecordType,
                    Name = proto
                }
            });
            return packet;
        }

        Mock<IServiceCore> BuildService(string ip4Address, string ip6Address = null)
        {
            var core = new Mock<IServiceCore>();

            List<IPAddress> addrs = new List<IPAddress>();
            if (!string.IsNullOrEmpty(ip6Address))
            {
                addrs.Add(IPAddress.Parse(ip6Address));
            }
            addrs.Add(IPAddress.Parse(ip4Address));
            core.Setup(x => x.Addresses).Returns(addrs);

            var nic = new Mock<NetworkInterface>();

            core.Setup(x => x.Network).Returns(nic.Object);
            return core;
        }

        [TestMethod]
        public void SearchForUnusedName()
        {
            var core = BuildService("10.0.0.1");
            core.Setup(x => x.SendPacket(It.IsAny<Packet>())).Callback<Packet>(packet => 
            {
                if (packet.Queries[0].Record.Name == "Scooby.local")
                {
                    var resp = BuildAnswerPacket("Scooby.local", "10.99.99.99");
                    core.Raise(x => x.PacketReceived += null, resp, new IPEndPoint(IPAddress.Parse("10.99.99.99"), 5353));
                }
            });

            using (var service = new ServicePublisher(core.Object))
            {
                string newName = null;
                service.LocalName = "Scooby";
                service.NameUpdated += x => newName = x;
                service.Start();
                Assert.IsFalse(string.IsNullOrEmpty(newName));
            }
        }

        Packet BuildAnswerPacket(string name, string ip4, string ip6 = null)
        {
            var resp = new Packet();
            resp.IsQuery = false;

            if (!string.IsNullOrEmpty(ip4))
            {
                var answer = new Answer();
                answer.Record = new Record();
                answer.Record.Name = name;
                answer.Record.RecordType = AAnswer.RecordType; 
                var answerData = new AAnswer();
                answerData.Address = IPAddress.Parse("10.99.99.99");
                answer.Data = answerData;
                resp.Answers.Add(answer);
            }

            if (!string.IsNullOrEmpty(ip6))
            {
                var answer = new Answer();
                answer.Record = new Record();
                answer.Record.Name = name;
                answer.Record.RecordType = AAAAAnswer.RecordType;
                var answerData = new AAAAAnswer();
                answerData.Address = IPAddress.Parse(ip6);
                answer.Data = answerData;
                resp.Answers.Add(answer);
            }

            return resp;
        }
    }
}
