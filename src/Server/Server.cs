using Newtonsoft.Json;
using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using Zenject;
using DataPuller.Client;

namespace DataPuller.Server
{
    class Server : /*IInitializable,*/ IDisposable
    {
        /*public static event Action<string> _SendError;
        public static void SendError(string error) { _SendError?.Invoke(error); }*/

        private WebSocketServer webSocketServer = new WebSocketServer("ws://0.0.0.0:2946");

        public Server(){ Initialize(); }

        public void Initialize()
        {
            webSocketServer.AddWebSocketService<MapDataServer>("/BSDataPuller/MapData");
            webSocketServer.AddWebSocketService<LiveDataServer>("/BSDataPuller/LiveData");
            //webSocketServer.AddWebSocketService<ErrorServer>("/BSDataPuller/Error");
            webSocketServer.Start();
        }

        private static bool MapDataServerInitalized = false;
        internal class MapDataServer : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                if (MapDataServerInitalized) { Send(JsonConvert.SerializeObject(new MapData.JsonData(), Formatting.Indented)); }
                if (!MapDataServerInitalized)
                {
                    MapDataServerInitalized = true;
                    MapData.Update += (data) =>
                    {
                        #if DEBUG
                        Plugin.Logger.Debug(data);
                        #endif
                        Sessions.Broadcast(data);
                    };
                }
            }
        }

        private static bool LiveDataServerInitalized = false;
        internal class LiveDataServer : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                if (LiveDataServerInitalized) { Send(JsonConvert.SerializeObject(new LiveData.JsonData(), Formatting.Indented)); }
                if (!LiveDataServerInitalized)
                {
                    LiveDataServerInitalized = true;
                    LiveData.Update += (data) =>
                    {
                        #if DEBUG
                        Plugin.Logger.Debug(data);
                        #endif
                        Sessions.Broadcast(data);
                    };
                    Sessions.CloseSession(ID);
                }
            }
        }

        /*private static bool ErrorServerInitalized = false;
        internal class ErrorServer : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                if (!ErrorServerInitalized)
                {
                    ErrorServerInitalized = true;
                    _SendError += (data) =>
                    {
                        #if DEBUG
                        Plugin.Logger.Debug(data);
                        #endif
                        Sessions.Broadcast(data);
                    };
                    Sessions.CloseSession(ID);
                }
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
