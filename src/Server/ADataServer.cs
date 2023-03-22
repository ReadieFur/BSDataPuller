using System;
using System.Reflection;
using DataPuller.Data;
using IPA.Utilities.Async;
using WebSocketSharp;

#nullable enable
namespace DataPuller.Server
{
    internal class ADataServer<TData> : QueuedWebSocketBehavior where TData : AData
    {
        private readonly TData data;

        public ADataServer()
        {
            Plugin.Logger.Debug($"Initialize {typeof(TData).Name} endpoint.");
            //Get the singleton instance of TData (assume that TData has a static Instance property).
            data = typeof(TData).GetField("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as TData
                ?? throw new NullReferenceException($"Couldn't find the singleton instance of {typeof(TData).Name}.");
        }

        private void OnData(string data)
        {
#if DEBUG
            Plugin.Logger.Trace(data);
#endif
            QueuedSend(data);
        }

        protected override void OnOpen()
        {
            string jsonData = UnityMainThreadTaskScheduler.Factory.StartNew(() => data.ToJson()).Result;
            QueuedSend(jsonData);
            data.OnUpdate += OnData;
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            data.OnUpdate -= OnData;
        }
    }
}
