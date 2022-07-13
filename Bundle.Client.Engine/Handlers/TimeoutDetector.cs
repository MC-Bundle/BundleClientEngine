//using Bundle.Abstractions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Bundle.Client.Handlers
//{
//    public class TimeoutDetector : ITimeoutDetector
//    {
//        private DateTime _lastStateUpdate;
//        private object _lastStateUpdateLock = new object();

//        public event ITimeoutDetector.TimeoutHandler OnTimeout;

//        public TimeoutDetector(IPermanentActionFactory permanentActionFactory)
//        {
//            var handler = permanentActionFactory.CreateHandler();
//            handler.OnUpdate += Handler_OnUpdate;
//            handler.Start();
//        }

//        private void Handler_OnUpdate()
//        {
//            StateUpdate();
//            do
//            {
//                Thread.Sleep(TimeSpan.FromSeconds(15));
//                lock (_lastStateUpdateLock)
//                {
//                    if (_lastStateUpdate.AddSeconds(30) < DateTime.Now)
//                    {
//                        OnTimeout();
//                    }
//                }
//            }
//            while (true);
//        }

//        public void StateUpdate()
//        {
//            lock (_lastStateUpdateLock)
//            {
//                _lastStateUpdate = DateTime.Now;
//            }
//        }
//    }
//}
