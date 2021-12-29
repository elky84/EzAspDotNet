using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EzAspDotNet.WebSocketManager
{
    public abstract class Handler
    {
        protected ConnectionManager ConnectionManager { get; set; }

        public Handler(ConnectionManager connectionManager)
        {
            ConnectionManager = connectionManager;
        }

#pragma warning disable 1998
        public virtual async Task OnConnected(WebSocket socket)
        {
            ConnectionManager.AddSocket(socket);
        }
#pragma warning restore 1998

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await ConnectionManager.RemoveSocket(ConnectionManager.GetId(socket));
        }

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendMessageAsync(WebSocket socket, object message)
        {
            await SendMessageAsync(socket, JsonConvert.SerializeObject(message));
        }


        public async Task SendMessageAsync(string socketId, string message)
        {
            await SendMessageAsync(ConnectionManager.GetSocketById(socketId), message);
        }


        public async Task SendMessageAsync(string socketId, object message)
        {
            await SendMessageAsync(ConnectionManager.GetSocketById(socketId), JsonConvert.SerializeObject(message));
        }


        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var pair in ConnectionManager.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open)
                    await SendMessageAsync(pair.Value, message);
            }
        }

        public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
    }
}
