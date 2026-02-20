using AiGateway.Dtos;
using AiGateway.Services;

namespace AiGateway.Endpoints;

/// <summary>
/// Ollama API gateway endpoints.
/// </summary>
public static class OllamaEndpoints
{
    public static void MapOllamaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/ollama")
            .WithTags("Ollama")
            .RequireAuthorization("ClientOnly");

        group.MapPost("/api/generate", Generate)
            .WithSummary("Generate completion")
            .WithDescription("Generates text from a model by forwarding to Ollama /api/generate. Supports streaming responses.")
            .Accepts<OllamaGenerateRequestDto>("application/json")
            .Produces<OllamaGenerateResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/api/chat", Chat)
            .WithSummary("Chat completion")
            .WithDescription("Generates chat responses by forwarding to Ollama /api/chat. Supports streaming responses.")
            .Accepts<OllamaChatRequestDto>("application/json")
            .Produces<OllamaChatResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/api/embed", Embed)
            .WithSummary("Generate embeddings")
            .WithDescription("Generates embeddings by forwarding to Ollama /api/embed.")
            .Accepts<OllamaEmbedRequestDto>("application/json")
            .Produces<OllamaEmbedResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/api/pull", PullModel)
            .WithSummary("Pull model")
            .WithDescription("Pulls a model from a registry by forwarding to Ollama /api/pull.")
            .Accepts<OllamaPullRequestDto>("application/json")
            .Produces<OllamaPullResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/api/push", PushModel)
            .WithSummary("Push model")
            .WithDescription("Pushes a model to a registry by forwarding to Ollama /api/push.")
            .Accepts<OllamaPushRequestDto>("application/json")
            .Produces<OllamaPushResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/api/create", CreateModel)
            .WithSummary("Create model")
            .WithDescription("Creates a model by forwarding to Ollama /api/create.")
            .Accepts<OllamaCreateRequestDto>("application/json")
            .Produces<OllamaCreateResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/api/copy", CopyModel)
            .WithSummary("Copy model")
            .WithDescription("Copies a model by forwarding to Ollama /api/copy.")
            .Accepts<OllamaCopyRequestDto>("application/json")
            .Produces<OllamaCopyResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapDelete("/api/delete", DeleteModel)
            .WithSummary("Delete model")
            .WithDescription("Deletes a model by forwarding to Ollama /api/delete.")
            .Accepts<OllamaDeleteRequestDto>("application/json")
            .Produces<OllamaDeleteResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapGet("/api/tags", ListTags)
            .WithSummary("List local models")
            .WithDescription("Lists local models by forwarding to Ollama /api/tags.")
            .Produces<OllamaTagsResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/api/show", ShowModel)
            .WithSummary("Show model details")
            .WithDescription("Shows model metadata by forwarding to Ollama /api/show.")
            .Accepts<OllamaShowRequestDto>("application/json")
            .Produces<OllamaShowResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapGet("/api/ps", ListRunningModels)
            .WithSummary("List running models")
            .WithDescription("Lists running models by forwarding to Ollama /api/ps.")
            .Produces<OllamaPsResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapGet("/api/version", GetVersion)
            .WithSummary("Get Ollama version")
            .WithDescription("Gets Ollama version information by forwarding to Ollama /api/version.")
            .Produces<OllamaVersionResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);
    }

    private static Task<IResult> Generate(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "ollama",
            "/api/generate",
            "ollama",
            "generate");
    }

    private static Task<IResult> Chat(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "ollama",
            "/api/chat",
            "ollama",
            "chat");
    }

    private static Task<IResult> Embed(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "ollama",
            "/api/embed",
            "ollama",
            "embed");
    }

    private static Task<IResult> PullModel(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "ollama",
            "/api/pull",
            "ollama",
            "pull");
    }

    private static Task<IResult> PushModel(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "ollama",
            "/api/push",
            "ollama",
            "push");
    }

    private static Task<IResult> CreateModel(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "ollama",
            "/api/create",
            "ollama",
            "create");
    }

    private static Task<IResult> CopyModel(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "ollama",
            "/api/copy",
            "ollama",
            "copy");
    }

    private static Task<IResult> DeleteModel(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "ollama",
            "/api/delete",
            "ollama",
            "delete");
    }

    private static Task<IResult> ListTags(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "ollama",
            "/api/tags",
            "ollama",
            "tags");
    }

    private static Task<IResult> ShowModel(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "ollama",
            "/api/show",
            "ollama",
            "show");
    }

    private static Task<IResult> ListRunningModels(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "ollama",
            "/api/ps",
            "ollama",
            "ps");
    }

    private static Task<IResult> GetVersion(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "ollama",
            "/api/version",
            "ollama",
            "version");
    }

    private static async Task<IResult> SelfCheck(HttpContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        var client = httpClientFactory.CreateClient("ollama");
        var model = configuration["Ollama:DefaultModel"] ?? "llama3.2";

        var versionResult = new { ok = false, status = 0, body = string.Empty };
        var generateResult = new { ok = false, status = 0, body = string.Empty };

        try
        {
            using var versionResponse = await client.GetAsync("api/version", context.RequestAborted);
            var versionBody = await versionResponse.Content.ReadAsStringAsync(context.RequestAborted);
            versionResult = new { ok = versionResponse.IsSuccessStatusCode, status = (int)versionResponse.StatusCode, body = versionBody };
        }
        catch (Exception ex)
        {
            versionResult = new { ok = false, status = 0, body = ex.Message };
        }

        try
        {
            var payload = new { model, prompt = "ping", stream = false };
            using var generateResponse = await client.PostAsJsonAsync("api/generate", payload, context.RequestAborted);
            var generateBody = await generateResponse.Content.ReadAsStringAsync(context.RequestAborted);
            generateResult = new { ok = generateResponse.IsSuccessStatusCode, status = (int)generateResponse.StatusCode, body = generateBody };
        }
        catch (Exception ex)
        {
            generateResult = new { ok = false, status = 0, body = ex.Message };
        }

        return Results.Ok(new
        {
            version = versionResult,
            generate = generateResult
        });
    }
}
