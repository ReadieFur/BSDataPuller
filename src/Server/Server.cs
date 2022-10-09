using System;
using DataPuller.Data;
using WebSocketSharp.Server;

#nullable enable
namespace DataPuller.Server
{
    internal class Server : IDisposable
    {
        public const string PROTOCOL = "ws";
        public const string HOST = "0.0.0.0";
        public const uint PORT = 2946;
        public const string PATH_PREFIX = "BSDataPuller/";

        private readonly WebSocketServer webSocketServer = new($"{PROTOCOL}://{HOST}:{PORT}");

        public Server() => Initialize();

        public void Initialize()
        {
            Plugin.Logger.Debug("Initialize Server.");
            webSocketServer.AddWebSocketService<ADataServer<MapData>>($"/{PATH_PREFIX}{nameof(MapData)}");
            webSocketServer.AddWebSocketService<ADataServer<LiveData>>($"/{PATH_PREFIX}{nameof(LiveData)}");
            webSocketServer.Start();
        }

        public void Dispose() => webSocketServer.Stop();
    }
}
