using System.Diagnostics;
using Serilog;

namespace AiGateway.Middleware;

/// <summary>
/// Logs request traffic with authentication context and proxy metadata.
/// </summary>
public class TrafficLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public TrafficLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        var isMaster = context.User.FindFirst("is_master")?.Value ?? "unknown";
        var keyId = context.User.FindFirst("key_id")?.Value ?? "unknown";
        var appName = context.User.FindFirst("app_name")?.Value ?? "unknown";
        var service = context.Items.TryGetValue("proxy_service", out var serviceValue)
            ? serviceValue?.ToString() ?? "unknown"
            : "unknown";
        var action = context.Items.TryGetValue("proxy_action", out var actionValue)
            ? actionValue?.ToString() ?? "unknown"
            : "unknown";

        Log.Information(
            "HTTP {Method} {Path} -> {StatusCode} in {ElapsedMs}ms; Service={Service}; Action={Action}; KeyId={KeyId}; AppName={AppName}; IsMaster={IsMaster}",
            context.Request.Method,
            context.Request.Path.Value ?? string.Empty,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            service,
            action,
            keyId,
            appName,
            isMaster);
    }
}

/// <summary>
/// Extension methods for traffic logging.
/// </summary>
public static class TrafficLoggingExtensions
{
    public static IApplicationBuilder UseTrafficLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TrafficLoggingMiddleware>();
    }
}
