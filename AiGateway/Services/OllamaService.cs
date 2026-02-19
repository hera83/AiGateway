using AiGateway.Dtos;
using AiGateway.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiGateway.Services;

public sealed class OllamaService : IOllamaService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly OllamaOptions _opt;
    private readonly ILogger<OllamaService> _log;

    public OllamaService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options,
        ILogger<OllamaService> logger)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _opt = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _log = logger ?? throw new ArgumentNullException(nameof(logger));

        if (_http.BaseAddress == null)
            _http.BaseAddress = new Uri(_opt.BaseUrl.TrimEnd('/') + "/");

        if (_http.Timeout == System.Threading.Timeout.InfiniteTimeSpan || _http.Timeout == default)
            _http.Timeout = TimeSpan.FromSeconds(Math.Max(5, _opt.RequestTimeoutSeconds));
    }

    public async Task<GenerateResponseDto> GenerateAsync(GenerateRequestDto request, CancellationToken ct = default)
    {
        request = EnsureGenerateDefaults(request, stream: false);

        using var res = await _http.PostAsJsonAsync("api/generate", request, JsonOpts, ct);
        await EnsureSuccessAsync(res, ct);
        return await ReadJsonAsync<GenerateResponseDto>(res, ct);
    }

    public async IAsyncEnumerable<GenerateStreamChunkResponseDto> GenerateStreamAsync(
        GenerateRequestDto request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        request = EnsureGenerateDefaults(request, stream: true);

        await foreach (var chunk in PostNdjsonStreamAsync<GenerateStreamChunkResponseDto>("api/generate", request, ct))
            yield return chunk;
    }

    public async Task<ChatResponseDto> ChatAsync(ChatRequestDto request, CancellationToken ct = default)
    {
        request = EnsureChatDefaults(request, stream: false);

        using var res = await _http.PostAsJsonAsync("api/chat", request, JsonOpts, ct);
        await EnsureSuccessAsync(res, ct);
        return await ReadJsonAsync<ChatResponseDto>(res, ct);
    }

    public async IAsyncEnumerable<ChatStreamChunkResponseDto> ChatStreamAsync(
        ChatRequestDto request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        request = EnsureChatDefaults(request, stream: true);

        await foreach (var chunk in PostNdjsonStreamAsync<ChatStreamChunkResponseDto>("api/chat", request, ct))
            yield return chunk;
    }

    public async Task<EmbedResponseDto> EmbedAsync(EmbedRequestDto request, CancellationToken ct = default)
    {
        if (request.Input == null)
            throw new ArgumentNullException(nameof(request.Input));

        if (string.IsNullOrWhiteSpace(request.Model))
            request.Model = _opt.DefaultModel;

        using var res = await _http.PostAsJsonAsync("api/embed", request, JsonOpts, ct);
        await EnsureSuccessAsync(res, ct);
        return await ReadJsonAsync<EmbedResponseDto>(res, ct);
    }

    public async Task<EmbeddingsResponseDto> EmbeddingsAsync(EmbeddingsRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Model))
            request.Model = _opt.DefaultModel;

        using var res = await _http.PostAsJsonAsync("api/embeddings", request, JsonOpts, ct);
        await EnsureSuccessAsync(res, ct);
        return await ReadJsonAsync<EmbeddingsResponseDto>(res, ct);
    }

    public async Task<ListModelsResponseDto> ListModelsAsync(CancellationToken ct = default)
    {
        using var res = await _http.GetAsync("api/tags", ct);
        await EnsureSuccessAsync(res, ct);
        return await ReadJsonAsync<ListModelsResponseDto>(res, ct);
    }

    public async Task<ShowModelResponseDto> ShowModelAsync(ShowModelRequestDto request, CancellationToken ct = default)
    {
        using var res = await _http.PostAsJsonAsync("api/show", request, JsonOpts, ct);
        await EnsureSuccessAsync(res, ct);
        return await ReadJsonAsync<ShowModelResponseDto>(res, ct);
    }

    public async Task<PullModelResponseDto> PullModelAsync(PullModelRequestDto request, CancellationToken ct = default)
    {
        request.Stream = false;

        using var res = await _http.PostAsJsonAsync("api/pull", request, JsonOpts, ct);
        await EnsureSuccessAsync(res, ct);
        return await ReadJsonAsync<PullModelResponseDto>(res, ct);
    }

    public async IAsyncEnumerable<PullModelResponseDto> PullModelStreamAsync(
        PullModelRequestDto request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        request.Stream = true;

        await foreach (var chunk in PostNdjsonStreamAsync<PullModelResponseDto>("api/pull", request, ct))
            yield return chunk;
    }

    public async Task DeleteModelAsync(DeleteModelRequestDto request, CancellationToken ct = default)
    {
        using var res = await _http.SendAsync(BuildJsonRequest(HttpMethod.Delete, "api/delete", request), ct);
        await EnsureSuccessAsync(res, ct);
    }

    public async Task CopyModelAsync(CopyModelRequestDto request, CancellationToken ct = default)
    {
        using var res = await _http.PostAsJsonAsync("api/copy", request, JsonOpts, ct);
        await EnsureSuccessAsync(res, ct);
    }

    public async Task<CreateModelResponseDto> CreateModelAsync(CreateModelRequestDto request, CancellationToken ct = default)
    {
        request.Stream = false;

        using var res = await _http.PostAsJsonAsync("api/create", request, JsonOpts, ct);
        await EnsureSuccessAsync(res, ct);
        return await ReadJsonAsync<CreateModelResponseDto>(res, ct);
    }

    public async IAsyncEnumerable<CreateModelResponseDto> CreateModelStreamAsync(
        CreateModelRequestDto request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        request.Stream = true;

        await foreach (var chunk in PostNdjsonStreamAsync<CreateModelResponseDto>("api/create", request, ct))
            yield return chunk;
    }

    private GenerateRequestDto EnsureGenerateDefaults(GenerateRequestDto req, bool stream)
    {
        if (string.IsNullOrWhiteSpace(req.Model))
            req.Model = _opt.DefaultModel;

        if (req.KeepAlive == null && _opt.DefaultKeepAlive != null)
            req.KeepAlive = _opt.DefaultKeepAlive;

        req.Stream = stream;
        return req;
    }

    private ChatRequestDto EnsureChatDefaults(ChatRequestDto req, bool stream)
    {
        if (string.IsNullOrWhiteSpace(req.Model))
            req.Model = _opt.DefaultModel;

        if (req.KeepAlive == null && _opt.DefaultKeepAlive != null)
            req.KeepAlive = _opt.DefaultKeepAlive;

        req.Stream = stream;
        return req;
    }

    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage res, CancellationToken ct)
    {
        var obj = await res.Content.ReadFromJsonAsync<T>(JsonOpts, ct);
        if (obj == null)
            throw new InvalidOperationException("Ollama returned empty JSON body.");
        return obj;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage res, CancellationToken ct)
    {
        if (res.IsSuccessStatusCode) return;

        var body = await SafeReadBodyAsync(res, ct);
        _log.LogWarning("Ollama API error {StatusCode}: {Body}", (int)res.StatusCode, body);

        throw new OllamaApiException(
            message: $"Ollama API call failed with HTTP {(int)res.StatusCode} ({res.ReasonPhrase}).",
            statusCode: (int)res.StatusCode,
            responseBody: body);
    }

    private static async Task<string> SafeReadBodyAsync(HttpResponseMessage res, CancellationToken ct)
    {
        try { return await res.Content.ReadAsStringAsync(ct); }
        catch { return "<unable to read response body>"; }
    }

    private static HttpRequestMessage BuildJsonRequest<T>(HttpMethod method, string relativeUrl, T payload)
    {
        var msg = new HttpRequestMessage(method, relativeUrl);
        var json = JsonSerializer.Serialize(payload, JsonOpts);
        msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return msg;
    }

    private async IAsyncEnumerable<T> PostNdjsonStreamAsync<T>(
        string relativeUrl,
        object payload,
        [EnumeratorCancellation] CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, relativeUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json")
        };

        using var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        await EnsureSuccessAsync(res, ct);

        await using var stream = await res.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null && !ct.IsCancellationRequested)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            T? item;
            try
            {
                item = JsonSerializer.Deserialize<T>(line, JsonOpts);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to parse NDJSON line from Ollama: {Line}", line);
                continue;
            }

            if (item != null)
                yield return item;
        }
    }
}
