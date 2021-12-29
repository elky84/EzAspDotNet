using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace EzAspDotNet.WebSocketManager
{
    public class Middleware
    {
        private readonly RequestDelegate _next;
        private Handler _webSocketHandler { get; set; }

        public Middleware(RequestDelegate next, Handler webSocketHandler)
        {
            _next = next;
            _webSocketHandler = webSocketHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await _webSocketHandler.OnConnected(socket);

            await Receive(socket, async (result, buffer) => await OnReceive(socket, result, buffer));
            //TODO - investigate the Kestrel exception thrown when this is the last middleware
            //await _next.Invoke(context);
        }

        private async Task OnReceive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            try
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await _webSocketHandler.ReceiveAsync(socket, result, buffer);
                    return;
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocketHandler.OnDisconnected(socket);
                    return;
                }
            }
            catch (System.Exception e)
            {
                Log.Logger.Error($"Catched Exception. [Message:{e.Message}] StackTrace:{e.StackTrace}");
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                       cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}
