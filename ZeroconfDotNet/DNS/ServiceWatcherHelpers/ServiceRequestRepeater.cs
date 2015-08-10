using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using ZeroconfDotNet.Utils;

namespace ZeroconfDotNet.DNS
{
    public class ServiceRequestRepeater : IDisposable
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

            if (service.Connected)
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
                //TODO: Unit test and fix this, should set back to _false when reconnect...
                //unless purposely stopped of course
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
            packet.Flags.IsResponse = false;
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
            finally
            {
                //Setup next request
                _timer.FireNext(delays[nextDelayIndex]);
                AdvanceDelay();
            }
        }

        private int nextDelayIndex = 0;

        //The RFCs recommend following a fibonacci sequence delay, up to 1 hour
        static readonly int[] delays = new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584 };
        void AdvanceDelay()
        {
            nextDelayIndex = (nextDelayIndex + 1) % delays.Length;
        }

        public void Dispose()
        {

        }
    }

}
