using System;
using System.Net;
using ZeroconfDotNet.DNS.Network;

namespace ZeroconfDotNet.DNS
{
    public interface IServiceCore
    {
        //Network Status monitoring
        event NetworkStatusChangedDelegate NetworkStatusChanged;
        bool Connected { get; }

        //Network info
        NetworkInfo Network { get; }

        //Packet Send/Receive
        void SendPacket(Packet p);
        void SendPacket(Packet p, IPEndPoint ep);
        event PacketReceivedDelegate PacketReceived;
        
        //Start the service 
        //TODO: Am I sure this is necessary?
        void Start();

        //Stop the service
        void Stop();
    }
}
