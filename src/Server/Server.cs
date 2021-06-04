using Newtonsoft.Json;
using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using DataPuller.Client;
using IPA.Utilities.Async;
using System.Threading;
using System.Threading.Tasks;

namespace DataPuller.Server
{
    class Server : /*IInitializable,*/ IDisposable
    {
        /*public static event Action<string> _SendError;
        public static void SendError(string error) { _SendError?.Invoke(error); }*/

        private WebSocketServer webSocketServer = new WebSocketServer("ws://0.0.0.0:2946");

        public Server() { Initialize(); }

        public void Initialize()
        {
            webSocketServer.AddWebSocketService<MapDataServer>("/BSDataPuller/MapData");
            webSocketServer.AddWebSocketService<LiveDataServer>("/BSDataPuller/LiveData");
            //webSocketServer.AddWebSocketService<ErrorServer>("/BSDataPuller/Error");
            webSocketServer.Start();
        }

        //Would this not benifit from creating the json data on a new thread?
        internal abstract class QueuedWebSocketBehavior : WebSocketBehavior
        {
            private Task readyToWrite = Task.CompletedTask;
            private readonly CancellationTokenSource connectionClosed = new CancellationTokenSource();

            ///<summary>Queue data to send on the websocket in-order. This method is thread-safe.</summary>
            protected void QueuedSend(string data)
            {
                var promise = new TaskCompletionSource<object>();
                var oldReadyToWrite = Interlocked.Exchange(ref readyToWrite, promise.Task);
                oldReadyToWrite.ContinueWith(t =>
                {
                    SendAsync(data, b =>
                    {
                        promise.SetResult(null);
                    });
                }, connectionClosed.Token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            protected override void OnClose(CloseEventArgs e)
            {
                connectionClosed.Cancel();
            }
        }

        internal class MapDataServer : QueuedWebSocketBehavior
        {
            private void OnData(string data)
            {
#if DEBUG
                Plugin.Logger.Debug(data);
#endif
                QueuedSend(data);
            }

            protected override void OnOpen()
            {
                var data = UnityMainThreadTaskScheduler.Factory.StartNew(() => new MapData.JsonData()).Result;
                //For the OnOpen send I will have the data formatted.
                QueuedSend(JsonConvert.SerializeObject(data, Formatting.Indented));
                MapData.Update += OnData;
            }

            protected override void OnClose(CloseEventArgs e)
            {
                base.OnClose(e);
                MapData.Update -= OnData;
            }
        }

        internal class LiveDataServer : QueuedWebSocketBehavior
        {
            private void OnData(string data)
            {
#if DEBUG
                Plugin.Logger.Debug(data);
#endif
                QueuedSend(data);
            }

            protected override void OnOpen()
            {
                var data = UnityMainThreadTaskScheduler.Factory.StartNew(() => new LiveData.JsonData()).Result;
                QueuedSend(JsonConvert.SerializeObject(data, Formatting.Indented));
                LiveData.Update += OnData;
            }

            protected override void OnClose(CloseEventArgs e)
            {
                base.OnClose(e);
                LiveData.Update -= OnData;
            }
        }

        /*
        internal class ErrorServer : QueuingWebSocketBehavior
        {
            private void OnData(string data)
            {
                #if DEBUG
                Plugin.Logger.Debug(data);
                #endif
                QueuedSend(data);
            }

            protected override void OnOpen()
            {
                _SendError += OnData;
            }

            protected override void OnClose(CloseEventArgs e)
            {
                base.OnClose(e);
                _SendError -= OnData;
            }

        }*/

        public void Dispose()
        {
            /* Any need to do this?
             * webSocketServer.RemoveWebSocketService("/BSDataPuller/MapData");
             * webSocketServer.RemoveWebSocketService("/BSDataPuller/LiveData");
            */
            webSocketServer.Stop();
        }
    }
}
