using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroconfDotNet.Utils
{
    public interface ITimer
    {
        void FireNext(int seconds);
        event Action Fired;
    }
}
