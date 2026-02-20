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

    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "x-api-key",
        "authorization",
        "cookie",
        "set-cookie",
        "x-forwarded-for",
        "x-forwarded-proto",
        "x-forwarded-host",
        "forwarded"
    };

    private static readonly HashSet<string> DiagnosticHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Server",
        "WWW-Authenticate",
        "Via",
        "X-Powered-By",
        "X-Frame-Options",
        "Content-Type",
        "Content-Length"
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

        // Read config for header stripping
        var config = context.RequestServices.GetRequiredService<IConfiguration>();
        var stripSensitiveHeaders = config.GetValue<bool>("Proxy:StripSensitiveHeaders", true);

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

            var forwardedHeaders = new List<string>();
            var droppedHeaders = new List<string>();

            foreach (var header in context.Request.Headers)
            {
                var headerName = header.Key;

                // Check if header should be dropped
                if (HopByHopHeaders.Contains(headerName))
                {
                    droppedHeaders.Add($"{headerName}(hop-by-hop)");
                    continue;
                }

                if (stripSensitiveHeaders && SensitiveHeaders.Contains(headerName))
                {
                    droppedHeaders.Add($"{headerName}(sensitive)");
                    continue;
                }

                if (headerName.Equals("Host", StringComparison.OrdinalIgnoreCase))
                {
                    droppedHeaders.Add($"{headerName}(host)");
                    continue;
                }

                if (headerName.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                {
                    droppedHeaders.Add($"{headerName}(content-length)");
                    continue;
                }

                // Forward the header
                if (upstreamRequest.Content != null && headerName.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                {
                    upstreamRequest.Content.Headers.TryAddWithoutValidation(headerName, header.Value.ToArray());
                    forwardedHeaders.Add(headerName);
                }
                else
                {
                    upstreamRequest.Headers.TryAddWithoutValidation(headerName, header.Value.ToArray());
                    forwardedHeaders.Add(headerName);
                }
            }

            // Debug logging (header names only, no values)
            if (forwardedHeaders.Count > 0 || droppedHeaders.Count > 0)
            {
                Log.Debug(
                    "Upstream {Service} {Action}: forwarded={ForwardedHeaders}, dropped={DroppedHeaders}",
                    areaName,
                    actionName,
                    string.Join(", ", forwardedHeaders),
                    string.Join(", ", droppedHeaders));
            }

            using var upstreamResponse = await client.SendAsync(
                upstreamRequest,
                HttpCompletionOption.ResponseHeadersRead,
                context.RequestAborted);

            if (!upstreamResponse.IsSuccessStatusCode)
            {
                var upstreamBody = await SafeReadBodyAsync(upstreamResponse, context.RequestAborted);
                var truncatedBody = Truncate(upstreamBody, 4096);

                var diagnosticHeaders = ExtractDiagnosticHeaders(upstreamResponse);

                Log.Warning(
                    "Upstream {Service} {Action} returned {StatusCode}. Headers: {DiagnosticHeaders}. Body: {Body}",
                    areaName,
                    actionName,
                    (int)upstreamResponse.StatusCode,
                    diagnosticHeaders,
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
            Log.Error(ex, "Upstream {Service} {Action} connection failed: {Message}", areaName, actionName, ex.Message);
            return ErrorResponseWriter.ToResult(
                context,
                StatusCodes.Status502BadGateway,
                "BAD_GATEWAY",
                $"Failed to connect to upstream service: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Log.Error("Upstream {Service} {Action} timeout", areaName, actionName);
            return ErrorResponseWriter.ToResult(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "BAD_GATEWAY",
                "Upstream service timeout");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Upstream {Service} {Action} unexpected error: {Message}", areaName, actionName, ex.Message);
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

    private static string ExtractDiagnosticHeaders(HttpResponseMessage response)
    {
        var headers = new List<string>();

        foreach (var header in response.Headers)
        {
            if (DiagnosticHeaders.Contains(header.Key))
            {
                headers.Add($"{header.Key}={string.Join(",", header.Value)}");
            }
        }

        foreach (var header in response.Content.Headers)
        {
            if (DiagnosticHeaders.Contains(header.Key))
            {
                headers.Add($"{header.Key}={string.Join(",", header.Value)}");
            }
        }

        return string.Join("; ", headers);
    }
}
