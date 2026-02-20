using AiGateway.Dtos;
using AiGateway.Services;

namespace AiGateway.Endpoints;

/// <summary>
/// Speaches API gateway endpoints.
/// </summary>
public static class SpeachesEndpoints
{
    public static void MapSpeachesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/speaches")
            .WithTags("Speaches")
            .RequireAuthorization("ClientOnly");

        group.MapPost("/audio/transcriptions", TranscribeAudio)
            .WithSummary("Transcribe audio")
            .WithDescription("Transcribes audio to text by forwarding to Speaches /v1/audio/transcriptions. Supports multipart or form-encoded requests.")
            .Accepts<SpeachesTranscriptionRequestDto>("application/x-www-form-urlencoded", "multipart/form-data")
            .Produces<SpeachesTranscriptionResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/audio/translations", TranslateAudio)
            .WithSummary("Translate audio")
            .WithDescription("Translates audio to text by forwarding to Speaches /v1/audio/translations. Supports multipart or form-encoded requests.")
            .Accepts<SpeachesTranslationRequestDto>("application/x-www-form-urlencoded", "multipart/form-data")
            .Produces<SpeachesTranslationResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapGet("/models", ListModels)
            .WithSummary("List local models")
            .WithDescription("Lists locally available models by forwarding to Speaches /v1/models.")
            .Produces<SpeachesListModelsResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapGet("/models/{modelId}", GetModel)
            .WithSummary("Get local model")
            .WithDescription("Gets a specific model by forwarding to Speaches /v1/models/{model_id}.")
            .Produces<SpeachesModelResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/models/{modelId}", DownloadModel)
            .WithSummary("Download remote model")
            .WithDescription("Downloads a remote model by forwarding to Speaches /v1/models/{model_id}.")
            .Produces<SpeachesModelActionResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapDelete("/models/{modelId}", DeleteModel)
            .WithSummary("Delete model")
            .WithDescription("Deletes a local model by forwarding to Speaches /v1/models/{model_id}.")
            .Produces<SpeachesModelActionResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapGet("/audio/models", ListAudioModels)
            .WithSummary("List audio models")
            .WithDescription("Lists audio-specific models by forwarding to Speaches /v1/audio/models.")
            .Produces<SpeachesListAudioModelsResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapGet("/audio/voices", ListAudioVoices)
            .WithSummary("List audio voices")
            .WithDescription("Lists available voices by forwarding to Speaches /v1/audio/voices.")
            .Produces<SpeachesListAudioVoicesResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/audio/speech", SynthesizeSpeech)
            .WithSummary("Synthesize speech")
            .WithDescription("Generates speech audio by forwarding to Speaches /v1/audio/speech.")
            .Accepts<SpeachesSpeechRequestDto>("application/json")
            .Produces<SpeachesSpeechResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/audio/speech/embedding", CreateSpeechEmbedding)
            .WithSummary("Create speech embedding")
            .WithDescription("Creates a speaker embedding by forwarding to Speaches /v1/audio/speech/embedding. Supports multipart or form-encoded requests.")
            .Accepts<SpeachesSpeechEmbeddingRequestDto>("application/x-www-form-urlencoded", "multipart/form-data")
            .Produces<SpeachesSpeechEmbeddingResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);
    }

    private static Task<IResult> TranscribeAudio(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        // Allow unlimited request size for audio upload
        var maxRequestBodySizeFeature = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
        if (maxRequestBodySizeFeature != null)
        {
            maxRequestBodySizeFeature.MaxRequestBodySize = null; // Unlimited
        }

        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            "/v1/audio/transcriptions",
            "speaches",
            "audio-transcriptions");
    }

    private static Task<IResult> TranslateAudio(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        // Allow unlimited request size for audio upload
        var maxRequestBodySizeFeature = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
        if (maxRequestBodySizeFeature != null)
        {
            maxRequestBodySizeFeature.MaxRequestBodySize = null; // Unlimited
        }

        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            "/v1/audio/translations",
            "speaches",
            "audio-translations");
    }

    private static Task<IResult> ListModels(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            "/v1/models",
            "speaches",
            "models-list");
    }

    private static Task<IResult> GetModel(HttpContext context, string modelId, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            $"/v1/models/{Uri.EscapeDataString(modelId)}",
            "speaches",
            "models-get");
    }

    private static Task<IResult> DownloadModel(HttpContext context, string modelId, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            $"/v1/models/{Uri.EscapeDataString(modelId)}",
            "speaches",
            "models-download");
    }

    private static Task<IResult> DeleteModel(HttpContext context, string modelId, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            $"/v1/models/{Uri.EscapeDataString(modelId)}",
            "speaches",
            "models-delete");
    }

    private static Task<IResult> ListAudioModels(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            "/v1/audio/models",
            "speaches",
            "audio-models-list");
    }

    private static Task<IResult> ListAudioVoices(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            "/v1/audio/voices",
            "speaches",
            "audio-voices-list");
    }

    private static Task<IResult> SynthesizeSpeech(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            "/v1/audio/speech",
            "speaches",
            "audio-speech");
    }

    private static Task<IResult> CreateSpeechEmbedding(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            "/v1/audio/speech/embedding",
            "speaches",
            "speech-embedding");
    }
}
