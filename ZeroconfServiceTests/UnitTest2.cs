using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZeroconfDotNet.DNS;
using ZeroconfDotNet.Utils;

namespace ZeroconfServiceTests
{
    class ServiceRequestRepeater : IDisposable
    {
        private readonly IServiceCore _service;
        private readonly ITimer _timer;
        private readonly string _proto;

        public ServiceRequestRepeater(IServiceCore service, string protocol, ITimer timer)
        {
            _service = service;
            _timer = timer;
            _proto = protocol;

            _timer.Fired += _timer_Fired;
            SendPacket();
        }

        void _timer_Fired()
        {
            SendPacket();
        }

        void SendPacket()
        {
            //Create Packet
            var packet = new Packet();
            packet.IsQuery = true;
            packet.Queries.Add(new Query()
            {
                IsMulticast = true,
                Record = new Record()
                {
                    Class = 1,
                    Name = _proto,
                    RecordType = 12,
                },
            });

            //Send it
            _service.SendPacket(packet);

            //Setup next request
            _timer.FireNext(delays[nextDelayIndex]);
            AdvanceDelay();
        }

        private int nextDelayIndex = 0;

        //The RFCs recommend following a fibonacci sequence delay, up to 1 hour
        const int[] delays = new[]{1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584};
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
        public void TestMethod1()
        {

        }
    }
}
