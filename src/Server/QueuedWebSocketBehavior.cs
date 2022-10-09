using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

#nullable enable
namespace DataPuller.Server
{
    internal abstract class QueuedWebSocketBehavior : WebSocketBehavior
    {
        private Task readyToWrite = Task.CompletedTask;
        private readonly CancellationTokenSource connectionClosed = new();

        /// <summary>
        /// Queue data to send on the websocket in-order. This method is thread-safe.
        /// </summary>
        protected void QueuedSend(string data)
        {
            TaskCompletionSource<object>? promise = new();
            Task? oldReadyToWrite = Interlocked.Exchange(ref readyToWrite, promise.Task);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            oldReadyToWrite.ContinueWith(t => SendAsync(data, b => promise.SetResult(null)),
                connectionClosed.Token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        protected override void OnClose(CloseEventArgs e) => connectionClosed.Cancel();
    }
}
