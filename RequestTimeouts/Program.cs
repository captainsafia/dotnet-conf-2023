using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Timeouts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRequestTimeouts(timeoutOptions =>
{
    timeoutOptions.AddPolicy("WriteTimeoutResponse", new RequestTimeoutPolicy
    {
        TimeoutStatusCode = StatusCodes.Status408RequestTimeout, // Default is 504 Gateway Timeout
        WriteTimeoutResponse = httpContext => httpContext.Response.WriteAsync("Timed out with WriteTimeoutResponse!"), // Default is write nothing
        Timeout = TimeSpan.FromSeconds(3), // Default is no timeout
    });
});
var app = builder.Build();

app.UseRequestTimeouts();

var timeouts = app.MapGroup("/timeouts");
timeouts.WithRequestTimeout("WriteTimeoutResponse");

timeouts.MapGet("/", DelayAsync);

// Writing a custom response prevents the "WriteTimeoutResponse
timeouts.MapGet("/custom-response", async (int? delay, CancellationToken requestAborted) => {
    try
    {
        await Task.Delay(TimeSpan.FromSeconds(delay ?? 5), requestAborted);
    }
    catch (TaskCanceledException)
    {
        return TypedResults.Content("Custom timeout!", "text/plain");
    }

    return TypedResults.Content("Custom no timeout!", "text/plain");
});

// The closest timeout configuration wins. Nothing is inherited from the "WriteTimeoutResponse" group policy.
timeouts.MapGet("/2-second-timespan", DelayAsync).WithRequestTimeout(new TimeSpan(2));
timeouts.MapGet("/2-second-attribute",
    [RequestTimeout(milliseconds: 2000)]
    (int? delay, ILogger<Program> logger, CancellationToken requestAborted) => DelayAsync(delay, logger, requestAborted));

app.Run();

static async Task<ContentHttpResult> DelayAsync(int? delay, ILogger<Program> logger, CancellationToken requestAborted)
{
    var startTime = Stopwatch.GetTimestamp();
    var delayTimespan = TimeSpan.FromSeconds(delay ?? 5);
    try
    {
        await Task.Delay(delayTimespan, requestAborted);
        logger.LogInformation("Delay completed! Attempted delay: {Delay}, Actual time: {Elapsed}", delayTimespan, Stopwatch.GetElapsedTime(startTime));
        return TypedResults.Content("No timeout!", "text/plain");
    }
    catch (TaskCanceledException)
    {
        logger.LogInformation("Timed out! Attempted delay: {Delay}, Actual time: {Elapsed}", delayTimespan, Stopwatch.GetElapsedTime(startTime));
        // Let the WriteTimeoutResponse RequestDelegate handle writing the timeout response.
        throw;
    }
}
