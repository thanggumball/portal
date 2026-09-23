using System.Diagnostics;

namespace StudentPortal.API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Use OnCompleted instead of logging right after await _next(context): if the request throws,
        // it jumps straight out to ExceptionHandlingMiddleware (which sits outside) and skips the code
        // after await. OnCompleted always runs right before the response is sent, even on exceptions.
        context.Response.OnCompleted(() =>
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "{Method} {Path} => {StatusCode} ({ElapsedMilliseconds}ms)",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
