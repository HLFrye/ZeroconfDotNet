using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroconfDotNet.DNS
{
    delegate void RequestUpdateDelegate(Tuple<string, int>[] updates);
    interface IServiceCache
    {
        void AddPacket(Packet p);
        event RequestUpdateDelegate RequestUpdate;
    }
}
