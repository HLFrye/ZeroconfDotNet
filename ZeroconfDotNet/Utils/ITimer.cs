using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoveryDotNet.Utils
{
    public interface ITimer
    {
        void FireNext(int seconds);
        event Action Fired;
    }
}
