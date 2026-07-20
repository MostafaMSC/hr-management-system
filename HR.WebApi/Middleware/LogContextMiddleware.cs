using Serilog.Context;
using System.Security.Claims;

namespace HR.WebApi.Middleware;

public class LogContextMiddleware
{
    private readonly RequestDelegate _next;

    public LogContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        using (LogContext.PushProperty("UserId", userId ?? "Anonymous"))
        using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
        {
            await _next(context);
        }
    }
}
