using Bundle.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bundle.Client.Sources
{
    public sealed class TimeoutDetectorSource : ISourceHandler
    {
        private Thread _thread;
        private DateTime _lastStateUpdate;
        private object _lastStateUpdateLock = new object();
        private readonly IMinecraftClient _minecraftClient;
        private bool _isStop;
        private CancellationToken _token;

        public TimeoutDetectorSource(IMinecraftClient minecraftClient)
        {
            _minecraftClient = minecraftClient;
            _minecraftClient.OnNextPacket += _minecraftClient_OnNextPacket;
        }

        private void _minecraftClient_OnNextPacket(Packet packet)
        {
            StateUpdate();
        }

        public event ISourceHandler.SourceDelegate OnSource;

        public void Start(CancellationToken token)
        {
            _token = token;
            _isStop = false;
            _thread = new Thread(new ThreadStart(Update));
            _thread.Name = "PermanentActionHandler";
            _thread.Start();
        }


        private void Update()
        {
            StateUpdate();
            do
            {
                if (_token.IsCancellationRequested)
                    return;
                Thread.Sleep(TimeSpan.FromSeconds(15));
                if (_token.IsCancellationRequested)
                    return;
                lock (_lastStateUpdateLock)
                {
                    if (_isStop)
                        return;

                    if (_lastStateUpdate.AddSeconds(30) < DateTime.Now)
                    {
                        OnSource(null);
                    }

                    if (_isStop)
                        return;
                }
            }
            while (true);
        }

        private void StateUpdate()
        {
            lock (_lastStateUpdateLock)
            {
                _lastStateUpdate = DateTime.Now;
            }
        }

        public void Stop()
        {
            _isStop = true;
            _thread = null;
        }
    }
}
