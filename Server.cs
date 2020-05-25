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
            //CheckForUpdates();

            WebSocketServer webSocketServer = new WebSocketServer("ws://127.0.0.1:2946");
            webSocketServer.AddWebSocketService<BSDataPuller>("/BSDataPuller");
            webSocketServer.Start();
            Task.Run(() => { using (var ws = new WebSocket("ws://127.0.0.1:2946/BSDataPuller")) { while (!ws.IsAlive) { ws.Connect(); } ws.Close(); } });
        }

        /*BSIPA updates instead
        private void CheckForUpdates()
        {
            Task.Run(() =>
            {
                string localVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string pendingDirectory = Directory.GetParent(Application.dataPath).ToString() + @"\IPA\Pending\Plugins\";

                try
                {
                    WebClient webClient = new WebClient();
                    List<AppsJson> version = JsonConvert.DeserializeObject<List<AppsJson>>
                        (webClient.DownloadString(new Uri("http://readie.globalgamingco.org/apps/apps.json")));

                    foreach(AppsJson appData in version)
                    {
                        if (appData.name == "DataPuller")
                        {
                            if (appData.version != localVersion)
                            {
                                Logger.log.Info("A new version of DataPuller has been found and is being downloaded.");
                                while (Directory.Exists(pendingDirectory)) { }
                                Directory.CreateDirectory(pendingDirectory);
                                webClient.DownloadFile(new Uri("http://readie.globalgamingco.org/apps/beatsaber/datapuller/DataPuller.dll"), pendingDirectory);
                                Logger.log.Notice("The latest version of DataPuller has been downloaded, it will be in use on the next launch of BeatSaber.");
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.log.Error(ex.ToString());
                    Logger.log.Error($"Failed to contact server for version updates. The current version is {localVersion}." +
                        "\nGo to http://readie.globalgamingco.org/apps/beatsaber/datapuller to check for a new version.");
                }
            });
        }*/
    }

    internal class AppsJson
    {
        public bool show { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
        public string version { get; set; }
    }
}
