using SignalRStatefulReconnect.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<ChatHub>("/chat", signalRConnectionOptions =>
{
    signalRConnectionOptions.AllowStatefulReconnects = true;
});

app.Run();
