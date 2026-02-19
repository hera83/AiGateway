using AiGateway.Dtos;

namespace AiGateway.Middleware;

/// <summary>
/// Helper for generating consistent ErrorDto responses.
/// </summary>
public static class ErrorResponseWriter
{
    public static ErrorDto Create(HttpContext context, string code, string message, object? details = null)
    {
        return new ErrorDto
        {
            Code = code,
            Message = message,
            TraceId = context.TraceIdentifier,
            Details = details
        };
    }

    public static IResult ToResult(HttpContext context, int statusCode, string code, string message, object? details = null)
    {
        var dto = Create(context, code, message, details);
        return Results.Json(dto, statusCode: statusCode);
    }

    public static Task WriteAsync(HttpContext context, int statusCode, string code, string message, object? details = null)
    {
        var dto = Create(context, code, message, details);
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(dto);
    }
}
