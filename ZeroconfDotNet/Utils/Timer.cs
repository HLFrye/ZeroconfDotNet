using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DiscoveryDotNet.Utils
{
    class TimerUtil : ITimer
    {
        private readonly Timer _timer;

        public TimerUtil()
        {
            _timer = new Timer(x => Fired());
        }

        public event Action Fired = delegate { };

        public void FireNext(int seconds)
        {
            _timer.Change(seconds, System.Threading.Timeout.Infinite);
        }
    }
}
