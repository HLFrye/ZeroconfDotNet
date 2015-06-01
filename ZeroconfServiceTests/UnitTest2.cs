using System;
using System.Net;
using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZeroconfDotNet.DNS;
using ZeroconfDotNet.DNS.Network;
using ZeroconfDotNet.Utils;
using Moq;

namespace ZeroconfServiceTests
{
    class ServiceRequestRepeater : IDisposable
    {
        private readonly IServiceCore _service;
        private readonly ITimer _timer;
        private readonly string _proto;
        protected bool _stopped;

        public ServiceRequestRepeater(IServiceCore service, string protocol, ITimer timer)
        {
            _service = service;
            _timer = timer;
            _proto = protocol;
            _stopped = false;

            _timer.Fired += _timer_Fired;
            _service.NetworkStatusChanged += _service_NetworkStatusChanged;

            if (_service.Connected == true)
            {
                SendPacket(false);
            }
        }

        void _service_NetworkStatusChanged(bool last, bool now)
        {
            if ((last == false) && (now == true))
            {
                nextDelayIndex = 0;
                SendPacket(false);
            }
            else if (now == false)
            {
                _stopped = true;
            }
        }

        void _timer_Fired()
        {
            SendPacket(true);
        }

        void SendPacket(bool isMultiCast)
        {
            if (_stopped)
                return;

            //Create Packet
            var packet = new Packet();
            packet.IsQuery = true;
            packet.Queries.Add(new Query()
            {
                IsMulticast = isMultiCast,
                Record = new Record()
                {
                    Class = 1,
                    Name = _proto,
                    RecordType = 12,
                },
            });

            //Send it
            try
            {
                _service.SendPacket(packet);
            }
            catch (SocketException ex)
            {
                //Likely temporary disconnect, try again next time
            }

            //Setup next request
            _timer.FireNext(delays[nextDelayIndex]);
            AdvanceDelay();
        }

        private int nextDelayIndex = 0;

        //The RFCs recommend following a fibonacci sequence delay, up to 1 hour
        static readonly int[] delays = new[]{1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584};
        void AdvanceDelay()
        {
            nextDelayIndex = -~nextDelayIndex % delays.Length;
        }

        public void Dispose()
        {
            
        }
    }

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

            Assert.IsTrue(sentPacket.IsQuery);
            Assert.AreEqual(12, sentPacket.Queries[0].Record.RecordType);
            Assert.AreEqual("_tcp.local", sentPacket.Queries[0].Record.Name);
            sentPacket = null;

            timerMock.Raise(x => x.Fired += null);
            Assert.IsTrue(sentPacket.IsQuery);
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
    }
}
