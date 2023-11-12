using Microsoft.AspNetCore.SignalR;

namespace SignalRStatefulReconnect;

public class QueryStringUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.GetHttpContext()?.Request.Query["name"];
}
