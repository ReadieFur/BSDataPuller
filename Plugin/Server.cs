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
        public void Init()
        {
            string nonRoutableAddress = "0.0.0.0";
            string localHost = "127.0.0.1";

            #region Setup webserver
            WebSocketServer webSocketServer = new WebSocketServer($"ws://{nonRoutableAddress}:2946");
            webSocketServer.AddWebSocketService<StaticDataServer>("/BSDataPuller/StaticData");
            webSocketServer.AddWebSocketService<LiveDataServer>("/BSDataPuller/LiveData");
            webSocketServer.Start();
            #endregion

            #region Initialize webserver
            using (var ws = new WebSocket($"ws://{localHost}:2946/BSDataPuller/StaticData")) { while (!ws.IsAlive) { ws.Connect(); } ws.Close(); }
            using (var ws = new WebSocket($"ws://{localHost}:2946/BSDataPuller/LiveData")) { while (!ws.IsAlive) { ws.Connect(); } ws.Close(); }
            #endregion
        }

        private static bool _localStaticDataConnected = false;
        internal class StaticDataServer : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                if (_localStaticDataConnected) { Send(JsonConvert.SerializeObject(new StaticData.JsonData(), Formatting.Indented)); }
                if (!_localStaticDataConnected)
                {
                    _localStaticDataConnected = true;
                    StaticData.Update += (data) => { Sessions.Broadcast(data); };
                }
            }
        }

        private static bool _localLiveDataConnected = false;
        internal class LiveDataServer : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                if (_localLiveDataConnected) { Send(JsonConvert.SerializeObject(new LiveData.JsonData(), Formatting.Indented)); }
                if (!_localLiveDataConnected)
                {
                    _localLiveDataConnected = true;
                    LiveData.Update += (data) => { Sessions.Broadcast(data); };
                }
            }
        }
    }
}
