var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Use(next =>
{
    return httpContext =>
    {
        app.Logger.LogInformation("Request path: '{Path}'", httpContext.Request.Path);
        return next(httpContext);
    };
});

// Uncomment app.UseRouting() to prevent the above middleware from getting short circuited.
//app.UseRouting();

app.MapShortCircuit(404, "ignored", "prefixes");

var faviconBytes = File.ReadAllBytes("favicon.ico");
app.MapGet("/favicon.ico", () => TypedResults.File(faviconBytes, "image/x-icon")).ShortCircuit();

app.Run();
