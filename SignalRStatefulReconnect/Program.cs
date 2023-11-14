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

app.Run();
