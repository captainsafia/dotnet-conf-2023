var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var myLogger = app.Services.GetRequiredService<ILogger<Program>>();

app.Use(next =>
{
    return httpContext =>
    {
        myLogger.LogInformation("Request path: '{Path}'", httpContext.Request.Path);
        return next(httpContext);
    };
});

// Uncomment app.UseRouting() to prevent the above middleware from getting short circuited.
//app.UseRouting();

app.MapGet("/favicon.ico", () => TypedResults.StatusCode(StatusCodes.Status404NotFound))
    .ShortCircuit();

app.MapGet("/", () => "Hello World!");

app.Run();
