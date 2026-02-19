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

        if (context.Response.HasStarted)
        {
            return;
        }

        if (context.Response.ContentLength.HasValue && context.Response.ContentLength.Value > 0)
        {
            return;
        }

        if (!string.IsNullOrEmpty(context.Response.ContentType))
        {
            return;
        }

        var statusCode = context.Response.StatusCode;
        if (statusCode == StatusCodes.Status400BadRequest)
        {
            await ErrorResponseWriter.WriteAsync(context, statusCode, "BAD_REQUEST", "Bad Request");
        }
        else if (statusCode == StatusCodes.Status401Unauthorized)
        {
            await ErrorResponseWriter.WriteAsync(context, statusCode, "UNAUTHORIZED", "Unauthorized");
        }
        else if (statusCode == StatusCodes.Status403Forbidden)
        {
            await ErrorResponseWriter.WriteAsync(context, statusCode, "FORBIDDEN", "Forbidden");
        }
        else if (statusCode == StatusCodes.Status404NotFound)
        {
            await ErrorResponseWriter.WriteAsync(context, statusCode, "NOT_FOUND", "Not Found");
        }
        else if (statusCode == StatusCodes.Status502BadGateway)
        {
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
