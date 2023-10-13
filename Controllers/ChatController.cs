using System.Net.WebSockets;
using System.Text;
using ASP.NET_HW_23.Models;
using ASP.NET_HW_23.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ASP.NET_HW_23.Controllers;

public class ChatController : ControllerBase {
    private static readonly Dictionary<string, WebSocket> Clients = new();
    private readonly IRepository<Message> _messageRepository;

    public ChatController(IRepository<Message> messageRepository) {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
    }

    [Route("/ws")]
    public async Task Get() {
        if (HttpContext.WebSockets.IsWebSocketRequest) {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var name = HttpContext.Request.Query["username"].ToString();
            if (!Clients.ContainsKey(name)) {
                Clients.Add(name, webSocket);
                await ListenAsync(name);
            }
            else {
                HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
        }
        else {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task ListenAsync(string name) {
        try {
            var serviceMessage = name + " - Connected.";
            var bufMessage = Encoding.UTF8.GetBytes(serviceMessage);
            foreach (var user in Clients.Keys) {
                await SendMessageAsync(user, bufMessage);
            }

            var client = Clients[name];
            var buffer = new byte [1024 * 4];

            var receivedResult = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None)
                .ConfigureAwait(false);
            while (!receivedResult.CloseStatus.HasValue) {
                var jsonMessage = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                var message = JsonConvert.DeserializeObject<Message>(jsonMessage);

                if (message != null) {
                    message.Sender = name;
                    jsonMessage = JsonConvert.SerializeObject(message);
                    bufMessage = Encoding.UTF8.GetBytes(jsonMessage);

                    if (string.IsNullOrEmpty(message.Recipient)) {
                        foreach (var user in Clients.Keys.Where(user => user != name)) {
                            await SendMessageAsync(user, bufMessage);
                        }
                    }
                    else {
                        await SendMessageAsync(message.Recipient, bufMessage);
                    }

                    await _messageRepository.InsertAsync(message);
                }

                receivedResult = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None)
                    .ConfigureAwait(false);
            }
            await CloseConnectionTo(name);
        }
        catch (Exception) {
            await CloseConnectionTo(name);
        }
    }

    private static async Task SendMessageAsync(string name, byte[] array) {
        try {
            if (!Clients.TryGetValue(name, out var socket)) return;

            if (socket.State == WebSocketState.Open) {
                await socket.SendAsync(array, WebSocketMessageType.Text, true, CancellationToken.None);
            }

            if (socket.State == WebSocketState.Closed) {
                await CloseConnectionTo(name);
            }
        }
        catch (ObjectDisposedException) {
            await CloseConnectionTo(name);
        }
    }

    private static async Task CloseConnectionTo(string name) {
        if (Clients.TryGetValue(name, out var socket)) {
            Clients.Remove(name);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }
}