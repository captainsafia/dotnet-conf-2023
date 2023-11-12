using Microsoft.AspNetCore.SignalR;

namespace SignalRStatefulReconnect.Hubs;

public class ChatHub : Hub
{
    public override Task OnConnectedAsync() =>
        Clients.All.SendAsync("Send", $"{Context.UserIdentifier} joined the chat");

    public override Task OnDisconnectedAsync(Exception? exception) =>
        Clients.All.SendAsync("Send", $"{Context.UserIdentifier} left the chat");

    public Task SendAll(string name, string message) =>
        Clients.All.SendAsync("Send", $"{name}: {message}");
}
