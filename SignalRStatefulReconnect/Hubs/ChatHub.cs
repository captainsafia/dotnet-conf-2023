using Microsoft.AspNetCore.SignalR;

namespace SignalRStatefulReconnect.Hubs;

public class ChatHub : Hub
{
    public override Task OnConnectedAsync()
    {
        var name = Context.GetHttpContext()?.Request.Query["name"];
        return Clients.All.SendAsync("Send", $"{name} joined the chat");
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var name = Context.GetHttpContext()?.Request.Query["name"];
        return Clients.All.SendAsync("Send", $"{name} left the chat");
    }

    public Task SendAll(string name, string message)
    {
        return Clients.All.SendAsync("Send", $"{name}: {message}");
    }
}
