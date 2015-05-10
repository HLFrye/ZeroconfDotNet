using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroconfDotNet.Utils
{
    class Timer : ITimer
    {
        public event Action Fired;
    }
}
