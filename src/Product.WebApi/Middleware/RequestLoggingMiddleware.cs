using System.Diagnostics;
using Microsoft.AspNetCore.Routing;

namespace Product.WebApi.Middleware;

public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Trace-Id"] = traceId;
            return Task.CompletedTask;
        });
        using var scope = logger.BeginScope(new Dictionary<string, object?> { ["traceId"] = traceId });
        var started = Stopwatch.GetTimestamp();
        try
        {
            await next(context);
        }
        finally
        {
            if (!context.Request.Path.StartsWithSegments("/health"))
            {
                var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "unmatched";
                logger.LogInformation("HTTP {Method} {Route} returned {StatusCode} in {ElapsedMs} ms",
                    context.Request.Method, route, context.Response.StatusCode,
                    Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            }
        }
    }
}
