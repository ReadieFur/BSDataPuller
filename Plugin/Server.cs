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
            string IP = "127.0.0.1";

            #region Create/read config file
            try
            {
                if (!File.Exists(Directory.GetParent(Application.dataPath) + "\\UserData\\DataPuller.txt"))
                {
                    using (StreamWriter sw = File.CreateText(Directory.GetParent(Application.dataPath) + "\\UserData\\DataPuller.txt"))
                    {
                        sw.WriteLine("#Change the ip for use over lan, app default is 127.0.0.1");
                        sw.WriteLine("ip=");
                    }
                }
                else
                {
                    using (StreamReader sr = new StreamReader(Directory.GetParent(Application.dataPath) + "\\UserData\\DataPuller.txt"))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line != "ip=" && line.StartsWith("ip="))
                            {
                                IP = line.Substring(3);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.log.Info(ex);
                File.Delete(Directory.GetParent(Application.dataPath) + "\\UserData\\DataPuller.txt");
                using (StreamWriter sw = File.CreateText(Directory.GetParent(Application.dataPath) + "\\UserData\\DataPuller.txt"))
                {
                    sw.WriteLine("#Change the ip for use over lan, app default is 127.0.0.1");
                    sw.WriteLine("ip=");
                }
            }
            #endregion

            #region Setup webserver
            WebSocketServer webSocketServer = new WebSocketServer($"ws://{IP}:2946");
            webSocketServer.AddWebSocketService<StaticDataServer>("/BSDataPuller/StaticData");
            webSocketServer.AddWebSocketService<LiveDataServer>("/BSDataPuller/LiveData");
            webSocketServer.Start();
            #endregion

            #region Initialize webserver
            using (var ws = new WebSocket($"ws://{IP}:2946/BSDataPuller/StaticData")) { while (!ws.IsAlive) { ws.Connect(); } ws.Close(); }
            using (var ws = new WebSocket($"ws://{IP}:2946/BSDataPuller/LiveData")) { while (!ws.IsAlive) { ws.Connect(); } ws.Close(); }
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
