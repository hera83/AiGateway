using Serilog;

namespace AiGateway.Middleware;

/// <summary>
/// Global error handling middleware that returns ErrorDto for all exceptions.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        Log.Error(exception, "Unhandled exception in {Path}: {Message}", context.Request.Path, exception.Message);

        // If response has already started, we cannot modify it
        if (context.Response.HasStarted)
        {
            Log.Warning("Response already started, cannot send error response");
            return Task.CompletedTask;
        }

        return ErrorResponseWriter.WriteAsync(
            context,
            StatusCodes.Status500InternalServerError,
            "INTERNAL_ERROR",
            "An unexpected error occurred");
    }
}

/// <summary>
/// Extension methods for error handling.
/// </summary>
public static class ErrorHandlingExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
