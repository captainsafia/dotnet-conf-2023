using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IExceptionHandler, LoggingExceptionHandler>();
builder.Services.AddSingleton<IExceptionHandler, LibraryExceptionHandler>();
var app = builder.Build();

app.UseExceptionHandler("/error");

app.MapGet("/", () => "Hello World!");
app.MapGet("/invalid", () =>
{
    throw new InvalidOperationException();
});
app.MapGet("/teapot", () =>
{
    throw new LibraryException("I am a teapot!");
});
app.MapGet("/error", (HttpContext context) =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();
    return $"{feature?.Error.GetType().Name} exception thrown handling a request at '{feature?.Path}'";
});

app.Run();

class LoggingExceptionHandler(ILogger<LoggingExceptionHandler> logger) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unandled exception at '{Path}'", httpContext.Request.Path);
        return ValueTask.FromResult(false);
    }
}

class LibraryExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is LibraryException)
        {
            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = StatusCodes.Status418ImATeapot;
                await httpContext.Response.WriteAsync("I'm a teapot!");
            }

            return true;
        }

        return false;
    }
}

class LibraryException(string message, Exception? innerException = null) : Exception(message, innerException)
{
}