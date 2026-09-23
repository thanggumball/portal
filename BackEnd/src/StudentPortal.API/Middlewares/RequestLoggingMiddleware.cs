using System.Diagnostics;
using System.Security.Claims;

namespace StudentPortal.API.Middlewares;

public sealed class RequestLoggingMiddleware
{
    private const long SlowRequestMs = 1000;

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            Write(context, stopwatch.ElapsedMilliseconds);
        }
    }

    private void Write(HttpContext context, long elapsedMs)
    {
        var statusCode = context.Response.StatusCode;
        var isSlow = elapsedMs >= SlowRequestMs;

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? context.User.FindFirstValue("sub")
                     ?? "anonymous";

        var level = statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ when isSlow => LogLevel.Warning,
            _ => LogLevel.Information
        };

        _logger.Log(level,
            "[{TraceId}] {Method} {Path}{Query} -> {StatusCode} in {ElapsedMs} ms | user={UserId} ip={Ip}{Slow}",
            context.TraceIdentifier,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            statusCode,
            elapsedMs,
            userId,
            context.Connection.RemoteIpAddress,
            isSlow ? " SLOW" : string.Empty);
    }
}
