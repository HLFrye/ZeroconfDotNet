using System;
namespace ZeroconfDotNet.DNS
{
    public interface IServiceCore
    {
        event PacketReceivedDelegate PacketReceived;
        void SendPacket(Packet p);
        void Start();
    }
}
