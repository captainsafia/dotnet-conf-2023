using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Timeouts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRequestTimeouts(timeoutOptions =>
{
    timeoutOptions.AddPolicy("WriteTimeoutResponse", new RequestTimeoutPolicy
    {
        TimeoutStatusCode = StatusCodes.Status408RequestTimeout, // Default is 504 Gateway Timeout
        WriteTimeoutResponse = httpContext =>
        {
            return httpContext.Response.WriteAsync("Timed out by WriteTimeoutResponse policy after 3 seconds!");
        }, // Default is write nothing
        Timeout = TimeSpan.FromSeconds(3), // Default is no timeout
    });

    timeoutOptions.DefaultPolicy = new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(7) ,// Default is no timeout
        WriteTimeoutResponse = httpContext =>
        {
            return httpContext.Response.WriteAsync("Timed out by the default policy after 7 seconds!");
        }, // Default is write nothing
    };
});
var app = builder.Build();

app.UseRequestTimeouts();
app.UseDefaultFiles();
app.UseStaticFiles();

var timeouts = app.MapGroup("/timeouts");
timeouts.WithRequestTimeout("WriteTimeoutResponse");

timeouts.MapGet("/default-policy", DelayAsync);
// Writing a custom response prevents the "WriteTimeoutResponse
timeouts.MapGet("/custom-response", async (int? delay, ILogger<Program> logger, HttpContext context) =>
{
    try
    {
        return await DelayAsync(delay, logger, context.RequestAborted);
    }
    catch (TaskCanceledException)
    {
        var policyName = context.GetEndpoint()?.Metadata.GetMetadata<RequestTimeoutAttribute>()?.PolicyName;
        return TypedResults.Content($"Custom timeout response! Policy name: {policyName}", "text/plain");
    }
});

// The closest timeout configuration wins. Nothing is inherited from the "WriteTimeoutResponse" group policy.
timeouts.MapGet("/2-second-timespan", DelayAsync).WithRequestTimeout(TimeSpan.FromSeconds(2));
timeouts.MapGet("/2-second-attribute",
    [RequestTimeout(milliseconds: 2000)]
    (int? delay, ILogger<Program> logger, CancellationToken requestAborted) => DelayAsync(delay, logger, requestAborted));

timeouts.MapGet("/disabled", DelayAsync).DisableRequestTimeout();

app.Use(next =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    return async httpContext =>
    {
        if (httpContext.Request.Path == "/middleware")
        {
            if (!int.TryParse(httpContext.Request.Query["delay"], out var delay))
            {
                delay = 5;
            };
            var contentResult = await DelayAsync(delay, logger, httpContext.RequestAborted);
            await httpContext.Response.WriteAsync(contentResult.ResponseContent!);
            return;
        }

        await next(httpContext);
    };
});

app.Run();

static async Task<ContentHttpResult> DelayAsync(int? delay, ILogger<Program> logger, CancellationToken requestAborted)
{
    var startTime = Stopwatch.GetTimestamp();
    var delayTimespan = TimeSpan.FromSeconds(delay ?? 5);
    try
    {
        await Task.Delay(delayTimespan, requestAborted);
        logger.LogInformation("Delay completed! Attempted delay: {Delay}, Actual time: {Elapsed}", delayTimespan, Stopwatch.GetElapsedTime(startTime));
        return TypedResults.Content($"No timeout! Delay: {delayTimespan}", "text/plain");
    }
    catch (TaskCanceledException)
    {
        logger.LogInformation("Timed out! Attempted delay: {Delay}, Actual time: {Elapsed}", delayTimespan, Stopwatch.GetElapsedTime(startTime));
        // Let the WriteTimeoutResponse RequestDelegate handle writing the timeout response.
        throw;
    }
}
