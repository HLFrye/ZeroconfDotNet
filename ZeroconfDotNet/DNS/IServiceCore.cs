using System;
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
        event PacketReceivedDelegate PacketReceived;
        
        //Start the service 
        //TODO: Am I sure this is necessary?
        void Start();
    }
}
