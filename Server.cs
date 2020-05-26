using System;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using IPA;
using BS_Utils;

namespace DataPuller
{
    class Server
    {
        private static bool appConnected = false;

        internal class BSDataPuller : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                if (!appConnected)
                {
                    appConnected = true;
                    LevelInfo.jsonUpdated += (json) => { Sessions.Broadcast(json); };
                }
            }
        }

        public void Start()
        {
            string IP = "127.0.0.1";

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

            WebSocketServer webSocketServer = new WebSocketServer($"ws://{IP}:2946");
            webSocketServer.AddWebSocketService<BSDataPuller>("/BSDataPuller");
            webSocketServer.Start();
            Task.Run(() => { using (var ws = new WebSocket($"ws://{IP}:2946/BSDataPuller")) { while (!ws.IsAlive) { ws.Connect(); } ws.Close(); } });
        }
    }
}
