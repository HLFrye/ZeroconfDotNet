using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using DiscoveryDotNet.DNS.Network;

namespace DiscoveryDotNet.DNS
{
    public interface IServiceCore
    {
        //Network Status monitoring
        event NetworkStatusChangedDelegate NetworkStatusChanged;
        bool Connected { get; }

        //Network info
        //NetworkInfo Network { get; }
        NetworkInterface Network { get; }
        IList<IPAddress> Addresses { get; }

        //Packet Send/Receive
        void SendPacket(Packet p);
        void SendPacket(Packet p, IPEndPoint ep);
        event PacketReceivedDelegate PacketReceived;
        event MalformedPacketReceivedDelegate MalformedPacketReceived;
        
        //Start the service 
        //TODO: Am I sure this is necessary?
        void Start();

        //Stop the service
        void Stop();
    }
}
