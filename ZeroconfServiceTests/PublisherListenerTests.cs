using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ZeroconfDotNet;
using ZeroconfDotNet.DNS;
using ZeroconfServiceTests.Utils;

namespace ZeroconfServiceTests
{
    [TestClass]
    public class PublisherListenerTests
    {
        [TestMethod]
        public void TransferServiceInfo()
        {
            var publishInfo = new ServiceInfo();
            publishInfo.Name = "Pubtest";
            publishInfo.Port = 9999;
            publishInfo.Priority = 10;
            publishInfo.Weight = 10;
            publishInfo.Flags.Add("TestFlag");
            publishInfo.Data["TestData"] = "TestValue";

            ServiceInfo recvInfo = null;

            var service = new ConnectedServiceMock("10.99.99.99");
            using (var publisher = new ServicePublisher(service))
            {
                var watcher = new ServiceWatchManager(service);
                publisher.AddService("_pubtest._tcp.local", publishInfo);
                publisher.Start();
                watcher.WatchService("_pubtest._tcp.local", x => recvInfo = x);

                Assert.AreEqual(publishInfo.Name, recvInfo.Name);
                Assert.AreEqual(publishInfo.Port, recvInfo.Port);
                Assert.AreEqual(publishInfo.Priority, recvInfo.Priority);
                Assert.AreEqual(publishInfo.Weight, recvInfo.Weight);
                Assert.AreEqual(publishInfo.Flags[0], recvInfo.Flags[0]);
                Assert.AreEqual(publishInfo.Data["TestData"], recvInfo.Data["TestData"]);
                Assert.AreEqual(IPAddress.Parse("10.99.99.99"), recvInfo.IP4Address);
            }

            Assert.IsFalse(service.Running);
        }
    }
}
