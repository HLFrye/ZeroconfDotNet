using System;
using System.Net;
using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiscoveryDotNet.DNS;
using DiscoveryDotNet.DNS.Network;
using DiscoveryDotNet.Utils;
using Moq;

namespace ZeroconfServiceTests
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestNormalRepeatPacketSending()
        {
            var serviceMock = new Mock<IServiceCore>();
            var timerMock = new Mock<ITimer>();

            serviceMock.Setup(x => x.Connected).Returns(true);
            Packet sentPacket = null;
            serviceMock.Setup(x => x.SendPacket(It.IsAny<Packet>())).Callback<Packet>(x => sentPacket = x);

            var sender = new ServiceRequestRepeater(serviceMock.Object, "_tcp.local", timerMock.Object);
            sender.Start();

            Assert.IsFalse(sentPacket.Flags.IsResponse);
            Assert.AreEqual(12, sentPacket.Queries[0].Record.RecordType);
            Assert.AreEqual("_tcp.local", sentPacket.Queries[0].Record.Name);
            sentPacket = null;

            timerMock.Raise(x => x.Fired += null);
            Assert.IsFalse(sentPacket.Flags.IsResponse);
            Assert.AreEqual(12, sentPacket.Queries[0].Record.RecordType);
            Assert.AreEqual("_tcp.local", sentPacket.Queries[0].Record.Name);
        }

        [TestMethod]
        public void TestUnicastAfterReconnect()
        {
            var serviceMock = new Mock<IServiceCore>();
            var timerMock = new Mock<ITimer>();

            serviceMock.Setup(x => x.Connected).Returns(true);
            Packet sentPacket = null;
            serviceMock.Setup(x => x.SendPacket(It.IsAny<Packet>())).Callback<Packet>(x => sentPacket = x);

            var sender = new ServiceRequestRepeater(serviceMock.Object, "_tcp.local", timerMock.Object);
            sender.Start();
            sentPacket = null;
            serviceMock.Raise(x => x.NetworkStatusChanged += null, false, true);

            Assert.IsFalse(sentPacket.Queries[0].IsMulticast);
            sentPacket = null;

            timerMock.Raise(x => x.Fired += null);
            Assert.IsTrue(sentPacket.Queries[0].IsMulticast);
        }

        [TestMethod]
        public void TestNoSendIfInitiallyDisconnect()
        {
            var serviceMock = new Mock<IServiceCore>();
            var timerMock = new Mock<ITimer>();
            serviceMock.Setup(x => x.Connected).Returns(false);

            Packet sentPacket = null;
            serviceMock.Setup(x => x.SendPacket(It.IsAny<Packet>())).Callback<Packet>(x => sentPacket = x);

            var sender = new ServiceRequestRepeater(serviceMock.Object, "_tcp.local", timerMock.Object);

            Assert.IsNull(sentPacket);
        }

        [TestMethod]
        public void TestNoSendAfterStop()
        {
            var serviceMock = new Mock<IServiceCore>();
            var timerMock = new Mock<ITimer>();
            serviceMock.Setup(x => x.Connected).Returns(true);

            Packet sentPacket = null;
            serviceMock.Setup(x => x.SendPacket(It.IsAny<Packet>())).Callback<Packet>(x => sentPacket = x);

            var sender = new ServiceRequestRepeater(serviceMock.Object, "_tcp.local", timerMock.Object);
            sender.Start();
            Assert.IsNotNull(sentPacket);
            sentPacket = null;
            sender.Stop(); // Need to add this
            timerMock.Raise(x => x.Fired += null);
            Assert.IsNull(sentPacket);
        }

        [TestMethod]
        public void TestNoSendIfReconnectAfterStop()
        {
            var serviceMock = new Mock<IServiceCore>();
            var timerMock = new Mock<ITimer>();
            serviceMock.Setup(x => x.Connected).Returns(true);

            Packet sentPacket = null;
            serviceMock.Setup(x => x.SendPacket(It.IsAny<Packet>())).Callback<Packet>(x => sentPacket = x);

            var sender = new ServiceRequestRepeater(serviceMock.Object, "_tcp.local", timerMock.Object);
            sender.Start();
            Assert.IsNotNull(sentPacket);
            sentPacket = null;
            sender.Stop();
            serviceMock.Raise(x => x.NetworkStatusChanged += null, false, true);
            timerMock.Raise(x => x.Fired += null);
            Assert.IsNull(sentPacket);
        }
    }
}
