using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace ZeroconfDotNet.DNS
{
    public delegate void PacketReceivedDelegate(Packet p, IPEndPoint endPoint);

    public class ServiceCore
    {
        private readonly Thread workerThread;
        private readonly Queue<Packet> packetQueue = new Queue<Packet>();
        private readonly object packetLock = new object();

        public ServiceCore()
        {
            workerThread = new Thread(new ThreadStart(ServiceThread));
            workerThread.IsBackground = true;
        }

        public void Start()
        {
            if (!workerThread.IsAlive)
                workerThread.Start();
        }

        public void Stop()
        {
            if (workerThread.IsAlive)            
                workerThread.Abort();            
        }

        public event PacketReceivedDelegate PacketReceived;

        public void SendPacket(Packet p)
        {
            var data = PacketWriter.Write(p);
            Client.Send(data, data.Length, new IPEndPoint(IPAddress.Parse("224.0.0.251"), 5353));
        }

        UdpClient Client;

        private void ServiceThread()
        {
            var client = new UdpClient();
            client.Client.ExclusiveAddressUse = false;
            client.Client.Bind(new IPEndPoint(IPAddress.Parse("192.168.16.1"), 5353));

            client.JoinMulticastGroup(IPAddress.Parse("224.0.0.251"));
            Client = client;
            Client.BeginReceive(new AsyncCallback(Receive), null);        
        }

        private void Receive(IAsyncResult res)
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8000);
            byte[] received = Client.EndReceive(res, ref RemoteIpEndPoint);
            PacketReceived(PacketReader.Read(received), RemoteIpEndPoint);                        
            Client.BeginReceive(new AsyncCallback(Receive), null);
        }
    }
}
