using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AiGateway.Dtos;

/// <summary>
/// Request DTO for audio transcriptions (multipart/form-data).
/// </summary>
public sealed class SpeachesTranscriptionRequestDto
{
    [Required]
    [MinLength(1)]
    public string Model { get; set; } = string.Empty;

    [Required]
    public IFormFile? File { get; set; }
    public string? Language { get; set; }
    public string? Prompt { get; set; }
    public string? ResponseFormat { get; set; }
    public double? Temperature { get; set; }
    public string[]? TimestampGranularities { get; set; }
    public bool? Stream { get; set; }
    public string? Hotwords { get; set; }
    public bool? WithoutTimestamps { get; set; }
}

public sealed class SpeachesTranscriptionResponseDto
{
    public JsonElement? Body { get; set; }
}

/// <summary>
/// Request DTO for audio translations (multipart/form-data).
/// </summary>
public sealed class SpeachesTranslationRequestDto
{
    [Required]
    [MinLength(1)]
    public string Model { get; set; } = string.Empty;

    [Required]
    public IFormFile? File { get; set; }
    public string? Prompt { get; set; }
    public string? ResponseFormat { get; set; }
    public double? Temperature { get; set; }
}

public sealed class SpeachesTranslationResponseDto
{
    public JsonElement? Body { get; set; }
}

/// <summary>
/// Request DTO for speech synthesis.
/// </summary>
public sealed class SpeachesSpeechRequestDto
{
    public string Model { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string Voice { get; set; } = string.Empty;
    public string? ResponseFormat { get; set; }
    public double? Speed { get; set; }
    public string? StreamFormat { get; set; }
    public int? SampleRate { get; set; }
}

public sealed class SpeachesSpeechResponseDto
{
    public string? ContentType { get; set; }
    public string? BodyBase64 { get; set; }
}

/// <summary>
/// Request DTO for speaker embedding (multipart/form-data).
/// </summary>
public sealed class SpeachesSpeechEmbeddingRequestDto
{
    [Required]
    [MinLength(1)]
    public string Model { get; set; } = string.Empty;

    [Required]
    public IFormFile? File { get; set; }
}

public sealed class SpeachesSpeechEmbeddingResponseDto
{
    public JsonElement? Body { get; set; }
}

public sealed class SpeachesListModelsResponseDto
{
    public JsonElement? Body { get; set; }
}

public sealed class SpeachesListAudioModelsResponseDto
{
    public JsonElement? Body { get; set; }
}

public sealed class SpeachesListAudioVoicesResponseDto
{
    public JsonElement? Body { get; set; }
}

public sealed class SpeachesModelResponseDto
{
    public JsonElement? Body { get; set; }
}

public sealed class SpeachesModelActionResponseDto
{
    public JsonElement? Body { get; set; }
}
