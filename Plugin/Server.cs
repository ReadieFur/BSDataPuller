using System;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.IO;
using DataPuller.GameData;
using Newtonsoft.Json;

namespace DataPuller
{
    class Server
    {
        class UserConfig
        { 
            //User config file as JSON, not needed yet.
        }

        public void Init()
        {
            string NonRoutableAddress = "0.0.0.0";
            string LocalHost = "127.0.0.1";

            #region Setup webserver
            WebSocketServer webSocketServer = new WebSocketServer($"ws://{NonRoutableAddress}:2946");
            webSocketServer.AddWebSocketService<StaticDataServer>("/BSDataPuller/StaticData");
            webSocketServer.AddWebSocketService<LiveDataServer>("/BSDataPuller/LiveData");
            webSocketServer.Start();
            #endregion

            #region Initialize webserver
            using (var ws = new WebSocket($"ws://{LocalHost}:2946/BSDataPuller/StaticData")) { while (!ws.IsAlive) { ws.Connect(); } ws.Close(); }
            using (var ws = new WebSocket($"ws://{LocalHost}:2946/BSDataPuller/LiveData")) { while (!ws.IsAlive) { ws.Connect(); } ws.Close(); }
            #endregion
        }

        private static bool LocalStaticDataConnected = false;
        internal class StaticDataServer : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                if (LocalStaticDataConnected) { Send(JsonConvert.SerializeObject(new StaticData.JsonData(), Formatting.Indented)); }
                if (!LocalStaticDataConnected)
                {
                    LocalStaticDataConnected = true;
                    StaticData.Update += (data) => { Sessions.Broadcast(data); };
                }
            }
        }

        private static bool LocalLiveDataConnected = false;
        internal class LiveDataServer : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                if (LocalLiveDataConnected) { Send(JsonConvert.SerializeObject(new LiveData.JsonData(), Formatting.Indented)); }
                if (!LocalLiveDataConnected)
                {
                    LocalLiveDataConnected = true;
                    LiveData.Update += (data) => { Sessions.Broadcast(data); };
                }
            }
        }
    }
}
