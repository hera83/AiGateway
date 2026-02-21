using AiGateway.Dtos;
using AiGateway.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

        // Audio processing endpoints (STT)
        group.MapPost("/audio/transcriptions", TranscribeAudio)
            .WithSummary("Transcribe audio (STT)")
            .WithDescription("Transcribes audio to text by forwarding to Speaches /v1/audio/transcriptions. Requires multipart/form-data with 'file' and 'model' fields.")
            .Accepts<SpeachesTranscriptionRequestDto>("multipart/form-data")
            .Produces<SpeachesTranscriptionResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/audio/translations", TranslateAudio)
            .WithSummary("Translate audio (STT)")
            .WithDescription("Translates audio to text by forwarding to Speaches /v1/audio/translations. Requires multipart/form-data with 'file' and 'model' fields.")
            .Accepts<SpeachesTranslationRequestDto>("multipart/form-data")
            .Produces<SpeachesTranslationResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        // Audio synthesis endpoints (TTS)
        group.MapPost("/audio/speech", SynthesizeSpeech)
            .WithSummary("Synthesize speech (TTS)")
            .WithDescription("Generates speech audio from text by forwarding to Speaches /v1/audio/speech.")
            .Accepts<SpeachesSpeechRequestDto>("application/json")
            .Produces<SpeachesSpeechResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/audio/speech/embedding", CreateSpeechEmbedding)
            .WithSummary("Create speech embedding")
            .WithDescription("Creates a speaker embedding by forwarding to Speaches /v1/audio/speech/embedding. Requires multipart/form-data with 'file' and 'model' fields.")
            .Accepts<SpeachesSpeechEmbeddingRequestDto>("multipart/form-data")
            .Produces<SpeachesSpeechEmbeddingResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        // Model management endpoints - Local installed models (shared: STT and TTS)
        group.MapGet("/models", ListModels)
            .WithSummary("List local installed models (STT & TTS)")
            .WithDescription("Lists all locally installed models (both STT and TTS) by forwarding to Speaches /v1/models.")
            .Produces<SpeachesListModelsResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapGet("/models/{modelId}", GetModel)
            .WithSummary("Get local model info (STT & TTS)")
            .WithDescription("Gets information about a specific locally installed model by forwarding to Speaches /v1/models/{model_id}. Example modelId: 'whisper-large-v3' or 'piper-en_US-lessac'.")
            .Produces<SpeachesModelResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapPost("/models/{modelId}", DownloadModel)
            .WithSummary("Download/install model (STT & TTS)")
            .WithDescription("Downloads and installs a model from registry by forwarding to Speaches /v1/models/{model_id}. Example modelId: 'whisper-large-v3' or 'piper-en_US-lessac'.")
            .Produces<SpeachesModelActionResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        group.MapDelete("/models/{modelId}", DeleteModel)
            .WithSummary("Delete local model (STT & TTS)")
            .WithDescription("Deletes a locally installed model by forwarding to Speaches /v1/models/{model_id}. Example modelId: 'whisper-large-v3' or 'piper-en_US-lessac'.")
            .Produces<SpeachesModelActionResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        // Available models from registry - STT
        group.MapGet("/models/available/stt", ListAvailableSttModels)
            .WithSummary("List available STT models (registry)")
            .WithDescription(@"Lists available STT models from Speaches registry.

Optional query parameters:
• q - Search filter (substring match in model ID, case-insensitive)
• language - Filter by language code (e.g., 'en', 'da', 'de')
• ownedBy - Filter by model owner/provider")
            .Produces<SpeachesRegistryResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status502BadGateway);

        // Available models from registry - TTS
        group.MapGet("/models/available/tts", ListAvailableTtsModels)
            .WithSummary("List available TTS models (registry)")
            .WithDescription(@"Lists available TTS models from Speaches registry (only models with voices).

Optional query parameters:
• q - Search filter (substring match in model ID, case-insensitive)
• language - Filter by language (matches model.language OR any voice language)
• voiceLanguage - Filter by voice language specifically
• ownedBy - Filter by model owner/provider")
            .Produces<SpeachesRegistryResponseDto>(StatusCodes.Status200OK)
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

    private static Task<IResult> GetModel(HttpContext context, [FromRoute] string modelId, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            $"/v1/models/{Uri.EscapeDataString(modelId)}",
            "speaches",
            "models-get");
    }

    private static Task<IResult> DownloadModel(HttpContext context, [FromRoute] string modelId, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            $"/v1/models/{Uri.EscapeDataString(modelId)}",
            "speaches",
            "models-download");
    }

    private static Task<IResult> DeleteModel(HttpContext context, [FromRoute] string modelId, IHttpClientFactory httpClientFactory)
    {
        return UpstreamForwarder.ForwardAsync(
            context,
            httpClientFactory,
            "speaches",
            $"/v1/models/{Uri.EscapeDataString(modelId)}",
            "speaches",
            "models-delete");
    }

    private static async Task<IResult> ListAvailableSttModels(
        HttpContext context, 
        IHttpClientFactory httpClientFactory,
        [FromQuery] string? q = null,
        [FromQuery] string? language = null,
        [FromQuery] string? ownedBy = null)
    {
        return await GetFilteredRegistryModelsAsync(
            context,
            httpClientFactory,
            "automatic-speech-recognition",
            q,
            language,
            null,
            ownedBy,
            null);
    }

    private static async Task<IResult> ListAvailableTtsModels(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        [FromQuery] string? q = null,
        [FromQuery] string? language = null,
        [FromQuery] string? voiceLanguage = null,
        [FromQuery] string? ownedBy = null)
    {
        return await GetFilteredRegistryModelsAsync(
            context,
            httpClientFactory,
            "text-to-speech",
            q,
            language,
            voiceLanguage,
            ownedBy,
            true); // Default: only show models with voices
    }

    private static async Task<IResult> GetFilteredRegistryModelsAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        string task,
        string? q,
        string? language,
        string? voiceLanguage,
        string? ownedBy,
        bool? hasVoices)
    {
        try
        {
            var client = httpClientFactory.CreateClient("speaches");
            var upstreamUrl = $"/v1/registry?task={Uri.EscapeDataString(task)}";

            using var upstreamRequest = new HttpRequestMessage(HttpMethod.Get, upstreamUrl);

            foreach (var header in context.Request.Headers)
            {
                if (header.Key.Equals("Accept", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Accept-Encoding", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Accept-Language", StringComparison.OrdinalIgnoreCase))
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
                var errorBody = await upstreamResponse.Content.ReadAsStringAsync(context.RequestAborted);
                return Results.Json(
                    new { error = "upstream_error", message = $"Speaches registry returned {(int)upstreamResponse.StatusCode}" },
                    statusCode: StatusCodes.Status502BadGateway);
            }

            var jsonString = await upstreamResponse.Content.ReadAsStringAsync(context.RequestAborted);
            using var jsonDoc = JsonDocument.Parse(jsonString);

            if (!jsonDoc.RootElement.TryGetProperty("data", out var dataArray) || dataArray.ValueKind != JsonValueKind.Array)
            {
                return Results.Json(new { data = Array.Empty<object>() });
            }

            var filtered = new List<JsonElement>();

            foreach (var model in dataArray.EnumerateArray())
            {
                if (!MatchesFilters(model, q, language, voiceLanguage, ownedBy, hasVoices, task))
                {
                    continue;
                }

                filtered.Add(model.Clone());
            }

            var result = new { data = filtered };
            return Results.Json(result);
        }
        catch (Exception ex)
        {
            return Results.Json(
                new { error = "registry_error", message = ex.Message },
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static bool MatchesFilters(
        JsonElement model,
        string? q,
        string? language,
        string? voiceLanguage,
        string? ownedBy,
        bool? hasVoices,
        string task)
    {
        if (!string.IsNullOrWhiteSpace(q))
        {
            if (model.TryGetProperty("id", out var idProp))
            {
                var id = idProp.GetString() ?? "";
                if (!id.Contains(q, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(ownedBy))
        {
            if (model.TryGetProperty("owned_by", out var ownedByProp))
            {
                var owned = ownedByProp.GetString() ?? "";
                if (!owned.Equals(ownedBy, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            if (task == "automatic-speech-recognition")
            {
                if (model.TryGetProperty("language", out var langArrayProp) && langArrayProp.ValueKind == JsonValueKind.Array)
                {
                    var hasMatch = false;
                    foreach (var lang in langArrayProp.EnumerateArray())
                    {
                        var langStr = lang.GetString() ?? "";
                        if (langStr.Equals(language, StringComparison.OrdinalIgnoreCase))
                        {
                            hasMatch = true;
                            break;
                        }
                    }
                    if (!hasMatch)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (task == "text-to-speech")
            {
                var matchInModelLanguage = false;
                if (model.TryGetProperty("language", out var modelLangProp))
                {
                    var modelLang = modelLangProp.GetString() ?? "";
                    if (modelLang.Equals(language, StringComparison.OrdinalIgnoreCase))
                    {
                        matchInModelLanguage = true;
                    }
                }

                var matchInVoiceLanguage = false;
                if (model.TryGetProperty("voices", out var voicesProp) && voicesProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var voice in voicesProp.EnumerateArray())
                    {
                        if (voice.TryGetProperty("language", out var voiceLangProp))
                        {
                            var voiceLang = voiceLangProp.GetString() ?? "";
                            if (voiceLang.Equals(language, StringComparison.OrdinalIgnoreCase))
                            {
                                matchInVoiceLanguage = true;
                                break;
                            }
                        }
                    }
                }

                if (!matchInModelLanguage && !matchInVoiceLanguage)
                {
                    return false;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(voiceLanguage) && task == "text-to-speech")
        {
            if (model.TryGetProperty("voices", out var voicesProp) && voicesProp.ValueKind == JsonValueKind.Array)
            {
                var hasMatch = false;
                foreach (var voice in voicesProp.EnumerateArray())
                {
                    if (voice.TryGetProperty("language", out var voiceLangProp))
                    {
                        var voiceLang = voiceLangProp.GetString() ?? "";
                        if (voiceLang.Equals(voiceLanguage, StringComparison.OrdinalIgnoreCase))
                        {
                            hasMatch = true;
                            break;
                        }
                    }
                }
                if (!hasMatch)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        if (hasVoices.HasValue && task == "text-to-speech")
        {
            if (model.TryGetProperty("voices", out var voicesProp) && voicesProp.ValueKind == JsonValueKind.Array)
            {
                var hasVoicesActual = voicesProp.GetArrayLength() > 0;
                if (hasVoices.Value != hasVoicesActual)
                {
                    return false;
                }
            }
            else
            {
                if (hasVoices.Value)
                {
                    return false;
                }
            }
        }

        return true;
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
            "/v1/audio/speech/embedding",
            "speaches",
            "speech-embedding");
    }
}
