using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace ZeroconfDotNet.DNS
{
    public delegate void PacketReceivedDelegate(Packet p);

    public class ServiceCore
    {
        private Thread workerThread;

        public ServiceCore()
        {
        }

        public event PacketReceivedDelegate PacketReceived;

        public void SendPacket(Packet p)
        {

        }

        private void ServiceThread()
        {
            var client = new UdpClient();
            client.Client.ExclusiveAddressUse = false;
            client.Client.Bind(new IPEndPoint(IPAddress.Parse("192.168.16.1"), 5353));

            client.JoinMulticastGroup(IPAddress.Parse("224.0.0.251"));            

            IPEndPoint remoteEndPoint = null;
            while (true)
            {
                var received = client.Receive(ref remoteEndPoint);
                try
                {
                    PacketReceived(PacketReader.Read(received));
                }
                catch (Exception)
                { }
            }

        }
    }
}
