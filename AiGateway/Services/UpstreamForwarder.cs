using AiGateway.Middleware;
using Serilog;

namespace AiGateway.Services;

/// <summary>
/// Shared helper for forwarding requests to upstream services with streaming support.
/// </summary>
public static class UpstreamForwarder
{
    private static readonly HashSet<string> HopByHopHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Connection",
        "Transfer-Encoding",
        "Keep-Alive",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "TE",
        "Trailer",
        "Upgrade"
    };

    public static async Task<IResult> ForwardAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        string clientName,
        string upstreamPath,
        string areaName,
        string actionName)
    {
        context.Items["proxy_service"] = areaName;
        context.Items["proxy_action"] = actionName;

        try
        {
            var client = httpClientFactory.CreateClient(clientName);
            var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
            var upstreamUri = new Uri($"{upstreamPath}{queryString}", UriKind.Relative);

            using var upstreamRequest = new HttpRequestMessage(new HttpMethod(context.Request.Method), upstreamUri);

            var method = context.Request.Method;
            var allowsBody = !HttpMethods.IsGet(method) && !HttpMethods.IsHead(method);
            if (allowsBody)
            {
                upstreamRequest.Content = new StreamContent(context.Request.Body);
            }

            foreach (var header in context.Request.Headers)
            {
                if (HopByHopHeaders.Contains(header.Key))
                {
                    continue;
                }

                if (header.Key.Equals("x-api-key", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (upstreamRequest.Content != null && header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                {
                    upstreamRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
                else
                {
                    upstreamRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            using var upstreamResponse = await client.SendAsync(
                upstreamRequest,
                HttpCompletionOption.ResponseHeadersRead,
                context.RequestAborted);

            if (!upstreamResponse.IsSuccessStatusCode)
            {
                var upstreamBody = await SafeReadBodyAsync(upstreamResponse, context.RequestAborted);
                var truncatedBody = Truncate(upstreamBody, 4096);

                Log.Warning(
                    "Upstream {Service} {Action} returned {StatusCode}. Body: {Body}",
                    areaName,
                    actionName,
                    (int)upstreamResponse.StatusCode,
                    truncatedBody);

                var status = upstreamResponse.StatusCode is System.Net.HttpStatusCode.ServiceUnavailable or System.Net.HttpStatusCode.GatewayTimeout
                    ? StatusCodes.Status503ServiceUnavailable
                    : StatusCodes.Status502BadGateway;

                return ErrorResponseWriter.ToResult(
                    context,
                    status,
                    "BAD_GATEWAY",
                    $"Upstream {areaName} returned HTTP {(int)upstreamResponse.StatusCode}",
                    new { upstreamStatus = (int)upstreamResponse.StatusCode, upstreamBody = truncatedBody });
            }

            context.Response.StatusCode = (int)upstreamResponse.StatusCode;

            foreach (var header in upstreamResponse.Headers)
            {
                if (HopByHopHeaders.Contains(header.Key))
                {
                    continue;
                }

                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in upstreamResponse.Content.Headers)
            {
                if (HopByHopHeaders.Contains(header.Key))
                {
                    continue;
                }

                if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                {
                    if (long.TryParse(header.Value.FirstOrDefault(), out var length))
                    {
                        context.Response.ContentLength = length;
                    }

                    continue;
                }

                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            if (!HttpMethods.IsHead(method))
            {
                await using var responseStream = await upstreamResponse.Content.ReadAsStreamAsync(context.RequestAborted);
                await responseStream.CopyToAsync(context.Response.Body, context.RequestAborted);
            }

            return Results.Empty;
        }
        catch (HttpRequestException ex)
        {
            return ErrorResponseWriter.ToResult(
                context,
                StatusCodes.Status502BadGateway,
                "BAD_GATEWAY",
                $"Failed to connect to upstream service: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ErrorResponseWriter.ToResult(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "BAD_GATEWAY",
                "Upstream service timeout");
        }
        catch (Exception)
        {
            return ErrorResponseWriter.ToResult(
                context,
                StatusCodes.Status502BadGateway,
                "BAD_GATEWAY",
                "Unexpected upstream error");
        }
    }

    private static async Task<string> SafeReadBodyAsync(HttpResponseMessage res, CancellationToken ct)
    {
        try { return await res.Content.ReadAsStringAsync(ct); }
        catch { return "<unable to read response body>"; }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value.Substring(0, maxLength);
    }
}
