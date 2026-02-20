using Serilog;

namespace AiGateway.Middleware;

/// <summary>
/// Ensures empty 4xx/5xx responses are converted to ErrorDto JSON.
/// </summary>
public class StatusCodeErrorMiddleware
{
    private readonly RequestDelegate _next;

    public StatusCodeErrorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Never write if response has already started
        if (context.Response.HasStarted)
        {
            return;
        }

        // Skip if response already has content
        if (context.Response.ContentLength.HasValue && context.Response.ContentLength.Value > 0)
        {
            return;
        }

        // Skip if content type is already set
        if (!string.IsNullOrEmpty(context.Response.ContentType))
        {
            return;
        }

        var statusCode = context.Response.StatusCode;

        if (statusCode == StatusCodes.Status400BadRequest)
        {
            Log.Warning("Returning 400 Bad Request for {Path}", context.Request.Path);
            await ErrorResponseWriter.WriteAsync(context, statusCode, "BAD_REQUEST", "Bad Request");
        }
        else if (statusCode == StatusCodes.Status401Unauthorized)
        {
            Log.Warning("Returning 401 Unauthorized for {Path} - missing or invalid API key", context.Request.Path);
            await ErrorResponseWriter.WriteAsync(context, statusCode, "UNAUTHORIZED", "Missing or invalid API key");
        }
        else if (statusCode == StatusCodes.Status403Forbidden)
        {
            var reason = "Access denied";
            var isMasterClaim = context.User.FindFirst("is_master")?.Value;
            var isMaster = isMasterClaim == "true";
            var isClient = isMasterClaim == "false";
            var keyId = context.User.FindFirst("key_id")?.Value;
            var appName = context.User.FindFirst("app_name")?.Value ?? "unknown";

            if (isMaster && context.Request.Path.StartsWithSegments("/v1/ollama", StringComparison.OrdinalIgnoreCase))
            {
                reason = "Master keys cannot access Ollama endpoints (client key required)";
            }
            else if (isMaster && context.Request.Path.StartsWithSegments("/v1/speaches", StringComparison.OrdinalIgnoreCase))
            {
                reason = "Master keys cannot access Speaches endpoints (client key required)";
            }
            else if (isClient && context.Request.Path.StartsWithSegments("/v1/keys", StringComparison.OrdinalIgnoreCase))
            {
                reason = "Client keys cannot manage API keys (master key required)";
            }

            Log.Warning(
                "Returning 403 Forbidden for {Method} {Path}: {Reason} (IsMaster={IsMaster}, KeyId={KeyId}, App={AppName})",
                context.Request.Method,
                context.Request.Path,
                reason,
                isMaster,
                keyId ?? "N/A",
                appName);

            await ErrorResponseWriter.WriteAsync(context, statusCode, "FORBIDDEN", reason);
        }
        else if (statusCode == StatusCodes.Status404NotFound)
        {
            Log.Warning("Returning 404 Not Found for {Path}", context.Request.Path);
            await ErrorResponseWriter.WriteAsync(context, statusCode, "NOT_FOUND", "Not Found");
        }
        else if (statusCode == StatusCodes.Status502BadGateway)
        {
            Log.Error("Returning 502 Bad Gateway for {Path}", context.Request.Path);
            await ErrorResponseWriter.WriteAsync(context, statusCode, "BAD_GATEWAY", "Bad Gateway");
        }
    }
}

/// <summary>
/// Extension methods for status code handling.
/// </summary>
public static class StatusCodeErrorExtensions
{
    public static IApplicationBuilder UseStatusCodeErrors(this IApplicationBuilder app)
    {
        return app.UseMiddleware<StatusCodeErrorMiddleware>();
    }
}
