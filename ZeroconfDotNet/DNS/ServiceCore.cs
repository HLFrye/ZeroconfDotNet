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

    public class ServiceCore : ZeroconfDotNet.DNS.IServiceCore
    {
        public ServiceCore()
        {
            var client = new UdpClient();
            client.Client.ExclusiveAddressUse = false;
            client.Client.Bind(new IPEndPoint(IPAddress.Parse("192.168.16.1"), 5353));

            client.JoinMulticastGroup(IPAddress.Parse("224.0.0.251"));
            Client = client;
        }
        
        public event PacketReceivedDelegate PacketReceived;

        public void SendPacket(Packet p)
        {
            var data = PacketWriter.Write(p);
            Client.Send(data, data.Length, new IPEndPoint(IPAddress.Parse("224.0.0.251"), 5353));
        }

        UdpClient Client;
        bool _started = false;

        public void Start()
        {
            if (!_started)
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
