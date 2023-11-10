using Microsoft.AspNetCore.Http.Timeouts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRequestTimeouts();
var app = builder.Build();

app.UseRequestTimeouts();

app.MapGet("/", [RequestTimeout(milliseconds: 2000)] async (CancellationToken requestAborted) => {
    try
    {
        await Task.Delay(TimeSpan.FromSeconds(10), requestAborted);
    }
    catch (TaskCanceledException)
    {
        return Results.Content("Timeout!", "text/plain");
    }

    return Results.Content("No timeout!", "text/plain");
});

app.Run();
