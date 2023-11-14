using Microsoft.AspNetCore.SignalR;
using SignalRStatefulReconnect;
using SignalRStatefulReconnect.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
});
builder.Services.AddSingleton<IUserIdProvider, QueryStringUserIdProvider>();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<ChatHub>("/chat", signalRConnectionOptions =>
{
    signalRConnectionOptions.AllowStatefulReconnects = true;
});

var httpContextLock = new object();
HttpContext? lastWebSocketHttpContext = null;

app.UseWebSockets();
app.Use(next =>
{
    return async httpContext =>
    {
        if (httpContext.WebSockets.IsWebSocketRequest)
        {
            Console.WriteLine($"Enter 'r' to reset {httpContext.Request.Query["name"]}'s WebSocket.");
            lastWebSocketHttpContext = httpContext;
        }

        await next(httpContext);

        lock (httpContextLock)
        {
            if (lastWebSocketHttpContext == httpContext)
            {
                lastWebSocketHttpContext = null;
            }
        }
    };
});
await app.StartAsync();
var lifetimeTask = app.WaitForShutdownAsync();

Console.WriteLine($"Enter 'q' to stop the server.");

while (!lifetimeTask.IsCompleted)
{
    var key = Console.Read();
    if (key == 'r')
    {
        lock (httpContextLock)
        {
            if (lastWebSocketHttpContext is not null)
            {
                Console.WriteLine("Reset WebSocket.");
                lastWebSocketHttpContext?.Abort();
            }
        }
    }
    else if (key == 'q')
    {
        await app.StopAsync();
    }
}
