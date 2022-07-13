using Bundle.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bundle.Client.Sources
{
    public class MinecraftClientSource : ISourceHandler
    {
        private readonly IMinecraftClient _minecraftClient;
        private readonly MinecraftContext _minecraftContext;
        private Thread _thread;
        private CancellationToken _token;

        public event ISourceHandler.SourceDelegate OnSource;

        public MinecraftClientSource(IMinecraftClient minecraftClient, MinecraftContext minecraftContext)
        {
            _minecraftClient = minecraftClient;
            _minecraftContext = minecraftContext;
        }

        public void Start(CancellationToken token)
        {
            _token = token;
            _thread = new Thread(new ThreadStart(Updater));
            _thread.Name = "PermanentActionHandler";
            _thread.Start();
        }

        private void Updater()
        {
            try
            {
                bool keepUpdating = true;
                Stopwatch stopWatch = new Stopwatch();
                while (keepUpdating)
                {
                    stopWatch.Start();
                    keepUpdating = Update();
                    stopWatch.Stop();
                    int elapsed = stopWatch.Elapsed.Milliseconds;
                    stopWatch.Reset();
                    if (elapsed < 100)
                        Thread.Sleep(100 - elapsed);
                }
            }
            catch (System.IO.IOException) { }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }

            _minecraftClient.Disconnect();
        }

        private bool Update()
        {
            if (!_minecraftClient.IsConnected())
                return false;
            try
            {
                while (_minecraftClient.HasDataAvailable())
                {
                    if (_token.IsCancellationRequested)
                        return false;

                    var packet = _minecraftClient.ReadNextPacket(_minecraftContext.CompressionTreshold, _minecraftContext.Encrypted);
                    OnSource(packet);
                }
            }
            catch (System.IO.IOException) { return false; }
            catch (SocketException) { return false; }
            catch (NullReferenceException) { return false; }
            catch (Ionic.Zlib.ZlibException) { return false; }
            return true;
        }

        public void Stop()
        {
            _thread = null;
        }

    }
}
