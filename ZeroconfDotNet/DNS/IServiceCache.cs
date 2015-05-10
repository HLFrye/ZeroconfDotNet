using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroconfDotNet.DNS
{
    public delegate void RequestUpdateDelegate(Tuple<string, int>[] updates);
    public interface IServiceCache
    {
        void AddPacket(Packet p);
        event RequestUpdateDelegate RequestUpdate;
    }
}
