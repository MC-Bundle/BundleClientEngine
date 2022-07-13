using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Handlers
{
    public interface ITimeoutDetector
    {
        public void StateUpdate();
        public delegate void TimeoutHandler();
        public event TimeoutHandler OnTimeout;
    }
}
