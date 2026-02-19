using System.Text.Json.Serialization;

namespace AiGateway.Dtos;

// ====================================================================
// GENERATE ENDPOINT - /api/generate
// ====================================================================

/// <summary>
/// Request DTO for /api/generate endpoint
/// </summary>
public sealed class GenerateRequestDto
{
    /// <summary>
    /// The model name to use (required)
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    /// <summary>
    /// The prompt to generate a response for (required)
    /// </summary>
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = default!;

    /// <summary>
    /// Optional: text to append after the generated response
    /// </summary>
    [JsonPropertyName("suffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Suffix { get; set; }

    /// <summary>
    /// Optional: list of base64-encoded images (for multimodal models)
    /// </summary>
    [JsonPropertyName("images")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Images { get; set; }

    /// <summary>
    /// Optional: (for thinking models) should the model think before responding?
    /// </summary>
    [JsonPropertyName("think")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Think { get; set; }

    /// <summary>
    /// Output format - "json" for JSON mode or a JSON schema object
    /// </summary>
    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Format { get; set; }

    /// <summary>
    /// Advanced generation options
    /// </summary>
    [JsonPropertyName("options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GenerationOptionsDto? Options { get; set; }

    /// <summary>
    /// System message to override the Modelfile default
    /// </summary>
    [JsonPropertyName("system")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? System { get; set; }

    /// <summary>
    /// Template to use (overrides Modelfile default)
    /// </summary>
    [JsonPropertyName("template")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Template { get; set; }

    /// <summary>
    /// Whether to stream responses (default: true)
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = true;

    /// <summary>
    /// If true, no formatting will be applied to the prompt
    /// </summary>
    [JsonPropertyName("raw")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Raw { get; set; }

    /// <summary>
    /// How long the model should stay loaded (e.g., "5m", "1h")
    /// </summary>
    [JsonPropertyName("keep_alive")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KeepAlive { get; set; }

    /// <summary>
    /// (Deprecated) Context from previous request for continuity
    /// </summary>
    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? Context { get; set; }

    /// <summary>
    /// Image width in pixels (for image generation models)
    /// </summary>
    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width { get; set; }

    /// <summary>
    /// Image height in pixels (for image generation models)
    /// </summary>
    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Height { get; set; }

    /// <summary>
    /// Number of diffusion steps (for image generation models)
    /// </summary>
    [JsonPropertyName("steps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Steps { get; set; }
}

/// <summary>
/// Response from /api/generate endpoint
/// </summary>
public sealed class GenerateResponseDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("response")]
    public string Response { get; set; } = default!;

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("total_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? TotalDuration { get; set; }

    [JsonPropertyName("load_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? LoadDuration { get; set; }

    [JsonPropertyName("prompt_eval_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PromptEvalCount { get; set; }

    [JsonPropertyName("prompt_eval_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? PromptEvalDuration { get; set; }

    [JsonPropertyName("eval_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EvalCount { get; set; }

    [JsonPropertyName("eval_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? EvalDuration { get; set; }

    /// <summary>
    /// Context for continuing generation in next request
    /// </summary>
    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? Context { get; set; }

    /// <summary>
    /// Reason why generation stopped (e.g., "stop", "length")
    /// </summary>
    [JsonPropertyName("done_reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DoneReason { get; set; }

    /// <summary>
    /// Generated image in base64 (for image generation models)
    /// </summary>
    [JsonPropertyName("image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Image { get; set; }
}

/// <summary>
/// Stream chunk from /api/generate
/// </summary>
public sealed class GenerateStreamChunkResponseDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("response")]
    public string Response { get; set; } = default!;

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("total_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? TotalDuration { get; set; }

    [JsonPropertyName("load_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? LoadDuration { get; set; }

    [JsonPropertyName("prompt_eval_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PromptEvalCount { get; set; }

    [JsonPropertyName("prompt_eval_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? PromptEvalDuration { get; set; }

    [JsonPropertyName("eval_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EvalCount { get; set; }

    [JsonPropertyName("eval_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? EvalDuration { get; set; }

    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? Context { get; set; }

    [JsonPropertyName("done_reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DoneReason { get; set; }

    [JsonPropertyName("completed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Completed { get; set; }

    [JsonPropertyName("total")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Total { get; set; }
}

// ====================================================================
// CHAT ENDPOINT - /api/chat
// ====================================================================

/// <summary>
/// Chat message in a conversation
/// </summary>
public sealed class ChatMessageDto
{
    /// <summary>
    /// Message role: "system", "user", "assistant", or "tool"
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = default!;

    /// <summary>
    /// Message content
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = default!;

    /// <summary>
    /// (For thinking models) the model's thinking process
    /// </summary>
    [JsonPropertyName("thinking")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Thinking { get; set; }

    /// <summary>
    /// Optional list of base64-encoded images (for multimodal models)
    /// </summary>
    [JsonPropertyName("images")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Images { get; set; }

    /// <summary>
    /// Optional list of tool calls made by the model
    /// </summary>
    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ToolCallDto>? ToolCalls { get; set; }

    /// <summary>
    /// Name of the tool that was executed (for tool results)
    /// </summary>
    [JsonPropertyName("tool_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolName { get; set; }
}

/// <summary>
/// Tool call made by the model
/// </summary>
public sealed class ToolCallDto
{
    [JsonPropertyName("function")]
    public ToolFunctionDto Function { get; set; } = default!;
}

/// <summary>
/// Tool function details
/// </summary>
public sealed class ToolFunctionDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("arguments")]
    public object? Arguments { get; set; }
}

/// <summary>
/// Request for /api/chat endpoint
/// </summary>
public sealed class ChatRequestDto
{
    /// <summary>
    /// Model name to use (required)
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    /// <summary>
    /// Chat messages (required)
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ChatMessageDto> Messages { get; set; } = new();

    /// <summary>
    /// Available tools for the model
    /// </summary>
    [JsonPropertyName("tools")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ToolDto>? Tools { get; set; }

    /// <summary>
    /// (For thinking models) should the model think before responding?
    /// </summary>
    [JsonPropertyName("think")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Think { get; set; }

    /// <summary>
    /// Output format - "json" for JSON mode or a JSON schema
    /// </summary>
    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Format { get; set; }

    /// <summary>
    /// Advanced generation options
    /// </summary>
    [JsonPropertyName("options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GenerationOptionsDto? Options { get; set; }

    /// <summary>
    /// Whether to stream responses (default: true)
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = true;

    /// <summary>
    /// How long the model should stay loaded
    /// </summary>
    [JsonPropertyName("keep_alive")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KeepAlive { get; set; }
}

/// <summary>
/// Response from /api/chat endpoint
/// </summary>
public sealed class ChatResponseDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("message")]
    public ChatMessageDto Message { get; set; } = default!;

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("total_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? TotalDuration { get; set; }

    [JsonPropertyName("load_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? LoadDuration { get; set; }

    [JsonPropertyName("prompt_eval_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PromptEvalCount { get; set; }

    [JsonPropertyName("prompt_eval_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? PromptEvalDuration { get; set; }

    [JsonPropertyName("eval_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EvalCount { get; set; }

    [JsonPropertyName("eval_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? EvalDuration { get; set; }

    [JsonPropertyName("done_reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DoneReason { get; set; }
}

/// <summary>
/// Stream chunk from /api/chat
/// </summary>
public sealed class ChatStreamChunkResponseDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("message")]
    public ChatMessageDto Message { get; set; } = default!;

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("total_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? TotalDuration { get; set; }

    [JsonPropertyName("load_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? LoadDuration { get; set; }

    [JsonPropertyName("prompt_eval_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PromptEvalCount { get; set; }

    [JsonPropertyName("prompt_eval_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? PromptEvalDuration { get; set; }

    [JsonPropertyName("eval_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EvalCount { get; set; }

    [JsonPropertyName("eval_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? EvalDuration { get; set; }

    [JsonPropertyName("done_reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DoneReason { get; set; }
}

// ====================================================================
// EMBEDDINGS ENDPOINT - /api/embed
// ====================================================================

/// <summary>
/// Request for /api/embed endpoint
/// </summary>
public sealed class EmbedRequestDto
{
    /// <summary>
    /// Model to use for embeddings (required)
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    /// <summary>
    /// Text or list of texts to embed (required)
    /// </summary>
    [JsonPropertyName("input")]
    public object Input { get; set; } = default!; // Can be string or List<string>

    /// <summary>
    /// Truncate input to fit within context length (default: true)
    /// </summary>
    [JsonPropertyName("truncate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Truncate { get; set; }

    /// <summary>
    /// Advanced generation options
    /// </summary>
    [JsonPropertyName("options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GenerationOptionsDto? Options { get; set; }

    /// <summary>
    /// How long the model should stay loaded
    /// </summary>
    [JsonPropertyName("keep_alive")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KeepAlive { get; set; }

    /// <summary>
    /// Number of dimensions for the embedding
    /// </summary>
    [JsonPropertyName("dimensions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Dimensions { get; set; }
}

/// <summary>
/// Response from /api/embed endpoint
/// </summary>
public sealed class EmbedResponseDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    [JsonPropertyName("embeddings")]
    public List<List<double>> Embeddings { get; set; } = new();

    [JsonPropertyName("total_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? TotalDuration { get; set; }

    [JsonPropertyName("load_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? LoadDuration { get; set; }

    [JsonPropertyName("prompt_eval_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PromptEvalCount { get; set; }
}

// ====================================================================
// MODEL CREATION - /api/create
// ====================================================================

/// <summary>
/// Request for /api/create endpoint
/// </summary>
public sealed class CreateModelRequestDto
{
    /// <summary>
    /// Model name to create (required)
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    /// <summary>
    /// Model to base new model on
    /// </summary>
    [JsonPropertyName("from")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? From { get; set; }

    /// <summary>
    /// File names and SHA256 digests of blobs
    /// </summary>
    [JsonPropertyName("files")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Files { get; set; }

    /// <summary>
    /// LORA adapters
    /// </summary>
    [JsonPropertyName("adapters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Adapters { get; set; }

    /// <summary>
    /// Prompt template
    /// </summary>
    [JsonPropertyName("template")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Template { get; set; }

    /// <summary>
    /// License or licenses for the model
    /// </summary>
    [JsonPropertyName("license")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? License { get; set; } // Can be string or List<string>

    /// <summary>
    /// System prompt
    /// </summary>
    [JsonPropertyName("system")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? System { get; set; }

    /// <summary>
    /// Model parameters
    /// </summary>
    [JsonPropertyName("parameters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Parameters { get; set; }

    /// <summary>
    /// Messages to create conversation
    /// </summary>
    [JsonPropertyName("messages")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ChatMessageDto>? Messages { get; set; }

    /// <summary>
    /// Whether to stream (default: true)
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = true;

    /// <summary>
    /// Quantization type (e.g., "q4_K_M", "q8_0")
    /// </summary>
    [JsonPropertyName("quantize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Quantize { get; set; }
}

/// <summary>
/// Response from /api/create endpoint
/// </summary>
public sealed class CreateModelResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("digest")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Digest { get; set; }

    [JsonPropertyName("total")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Total { get; set; }

    [JsonPropertyName("completed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Completed { get; set; }
}

// ====================================================================
// MODEL MANAGEMENT
// ====================================================================

/// <summary>
/// Request for /api/pull endpoint
/// </summary>
public sealed class PullModelRequestDto
{
    /// <summary>
    /// Model name to pull (required)
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    /// <summary>
    /// Allow insecure connections
    /// </summary>
    [JsonPropertyName("insecure")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Insecure { get; set; }

    /// <summary>
    /// Whether to stream (default: true)
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = true;
}

/// <summary>
/// Response from /api/pull endpoint
/// </summary>
public sealed class PullModelResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("digest")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Digest { get; set; }

    [JsonPropertyName("total")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Total { get; set; }

    [JsonPropertyName("completed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Completed { get; set; }
}

/// <summary>
/// Request for /api/push endpoint
/// </summary>
public sealed class PushModelRequestDto
{
    /// <summary>
    /// Model name in format namespace/model:tag (required)
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    /// <summary>
    /// Allow insecure connections
    /// </summary>
    [JsonPropertyName("insecure")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Insecure { get; set; }

    /// <summary>
    /// Whether to stream (default: true)
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = true;
}

/// <summary>
/// Response from /api/push endpoint
/// </summary>
public sealed class PushModelResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("digest")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Digest { get; set; }

    [JsonPropertyName("total")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Total { get; set; }

    [JsonPropertyName("completed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Completed { get; set; }
}

/// <summary>
/// Request for /api/copy endpoint
/// </summary>
public sealed class CopyModelRequestDto
{
    /// <summary>
    /// Source model name (required)
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = default!;

    /// <summary>
    /// Destination model name (required)
    /// </summary>
    [JsonPropertyName("destination")]
    public string Destination { get; set; } = default!;
}

/// <summary>
/// Request for /api/delete endpoint
/// </summary>
public sealed class DeleteModelRequestDto
{
    /// <summary>
    /// Model name to delete (required)
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;
}

/// <summary>
/// Request for /api/show endpoint
/// </summary>
public sealed class ShowModelRequestDto
{
    /// <summary>
    /// Model name to show (required)
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Return full data for verbose fields
    /// </summary>
    [JsonPropertyName("verbose")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Verbose { get; set; }
}

/// <summary>
/// Response from /api/show endpoint
/// </summary>
public sealed class ShowModelResponseDto
{
    [JsonPropertyName("modelfile")]
    public string Modelfile { get; set; } = default!;

    [JsonPropertyName("parameters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Parameters { get; set; }

    [JsonPropertyName("template")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Template { get; set; }

    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ModelDetailsDto? Details { get; set; }

    [JsonPropertyName("model_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? ModelInfo { get; set; }

    [JsonPropertyName("capabilities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Capabilities { get; set; }
}

/// <summary>
/// Response from /api/tags endpoint
/// </summary>
public sealed class ListModelsResponseDto
{
    [JsonPropertyName("models")]
    public List<ModelInfoDto> Models { get; set; } = new();
}

/// <summary>
/// Model info in list response
/// </summary>
public sealed class ModelInfoDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("digest")]
    public string Digest { get; set; } = default!;

    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ModelDetailsDto? Details { get; set; }
}

/// <summary>
/// Model details
/// </summary>
public sealed class ModelDetailsDto
{
    [JsonPropertyName("parent_model")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ParentModel { get; set; }

    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Format { get; set; }

    [JsonPropertyName("family")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Family { get; set; }

    [JsonPropertyName("families")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Families { get; set; }

    [JsonPropertyName("parameter_size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ParameterSize { get; set; }

    [JsonPropertyName("quantization_level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? QuantizationLevel { get; set; }
}

/// <summary>
/// Response from /api/ps endpoint (running models)
/// </summary>
public sealed class ListRunningModelsResponseDto
{
    [JsonPropertyName("models")]
    public List<RunningModelInfoDto> Models { get; set; } = new();
}

/// <summary>
/// Running model info
/// </summary>
public sealed class RunningModelInfoDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("digest")]
    public string Digest { get; set; } = default!;

    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ModelDetailsDto? Details { get; set; }

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [JsonPropertyName("size_vram")]
    public long SizeVram { get; set; }
}

/// <summary>
/// Response from /api/version endpoint
/// </summary>
public sealed class VersionResponseDto
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = default!;
}

// ====================================================================
// DEPRECATED: /api/embeddings (use /api/embed instead)
// ====================================================================

/// <summary>
/// Request for deprecated /api/embeddings endpoint
/// </summary>
public sealed class EmbeddingsRequestDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = default!;

    [JsonPropertyName("options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GenerationOptionsDto? Options { get; set; }

    [JsonPropertyName("keep_alive")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KeepAlive { get; set; }
}

/// <summary>
/// Response from deprecated /api/embeddings endpoint
/// </summary>
public sealed class EmbeddingsResponseDto
{
    [JsonPropertyName("embedding")]
    public List<double> Embedding { get; set; } = new();
}

// ====================================================================
// SHARED/COMMON MODELS
// ====================================================================

/// <summary>
/// Advanced generation options
/// </summary>
public sealed class GenerationOptionsDto
{
    [JsonPropertyName("num_keep")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NumKeep { get; set; }

    [JsonPropertyName("seed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Seed { get; set; }

    [JsonPropertyName("num_predict")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NumPredict { get; set; }

    [JsonPropertyName("top_k")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TopK { get; set; }

    [JsonPropertyName("top_p")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? TopP { get; set; }

    [JsonPropertyName("min_p")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? MinP { get; set; }

    [JsonPropertyName("typical_p")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? TypicalP { get; set; }

    [JsonPropertyName("repeat_last_n")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? RepeatLastN { get; set; }

    [JsonPropertyName("temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? Temperature { get; set; }

    [JsonPropertyName("repeat_penalty")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? RepeatPenalty { get; set; }

    [JsonPropertyName("presence_penalty")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? PresencePenalty { get; set; }

    [JsonPropertyName("frequency_penalty")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? FrequencyPenalty { get; set; }

    [JsonPropertyName("mirostat")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Mirostat { get; set; }

    [JsonPropertyName("mirostat_eta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? MirostatEta { get; set; }

    [JsonPropertyName("mirostat_tau")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? MirostatTau { get; set; }

    [JsonPropertyName("penalize_newline")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? PenalizeNewline { get; set; }

    [JsonPropertyName("stop")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Stop { get; set; }

    [JsonPropertyName("numa")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Numa { get; set; }

    [JsonPropertyName("num_ctx")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NumCtx { get; set; }

    [JsonPropertyName("num_batch")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NumBatch { get; set; }

    [JsonPropertyName("num_gpu")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NumGpu { get; set; }

    [JsonPropertyName("main_gpu")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MainGpu { get; set; }

    [JsonPropertyName("use_mmap")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? UseMmap { get; set; }

    [JsonPropertyName("num_thread")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NumThread { get; set; }

    [JsonPropertyName("tfs_z")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? TfsZ { get; set; }
}

/// <summary>
/// Tool definition for function calling
/// </summary>
public sealed class ToolDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public ToolDefinitionDto Function { get; set; } = default!;
}

/// <summary>
/// Tool function definition
/// </summary>
public sealed class ToolDefinitionDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonPropertyName("parameters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ToolParametersDto? Parameters { get; set; }
}

/// <summary>
/// Tool parameters schema
/// </summary>
public sealed class ToolParametersDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, ToolParameterPropertyDto>? Properties { get; set; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Required { get; set; }
}

/// <summary>
/// Tool parameter property
/// </summary>
public sealed class ToolParameterPropertyDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonPropertyName("enum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Enum { get; set; }
}

// ====================================================================
// PROXY-STYLE DTOs (for OllamaEndpoints transparent forwarding)
// These are simplified DTOs used in the gateway endpoints
// ====================================================================

public sealed class OllamaGenerateRequestDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Stream { get; set; }
}

public sealed class OllamaGenerateResponseDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}

public sealed class OllamaChatRequestDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<ChatMessageDto> Messages { get; set; } = new();

    [JsonPropertyName("stream")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Stream { get; set; }
}

public sealed class OllamaChatResponseDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public ChatMessageDto Message { get; set; } = default!;

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}

public sealed class OllamaEmbedRequestDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public object Input { get; set; } = default!;
}

public sealed class OllamaEmbedResponseDto
{
    [JsonPropertyName("embeddings")]
    public List<List<double>>? Embeddings { get; set; }
}

public sealed class OllamaPullRequestDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Stream { get; set; }
}

public sealed class OllamaPullResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public sealed class OllamaPushRequestDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Stream { get; set; }
}

public sealed class OllamaPushResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public sealed class OllamaCreateRequestDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("modelfile")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Modelfile { get; set; }

    [JsonPropertyName("stream")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Stream { get; set; }
}

public sealed class OllamaCreateResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public sealed class OllamaCopyRequestDto
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;
}

public sealed class OllamaCopyResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public sealed class OllamaDeleteRequestDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
}

public sealed class OllamaDeleteResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public sealed class OllamaTagsResponseDto
{
    [JsonPropertyName("models")]
    public List<ModelInfoDto>? Models { get; set; }
}

public sealed class OllamaShowRequestDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public sealed class OllamaShowResponseDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("license")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? License { get; set; }
}

public sealed class OllamaPsResponseDto
{
    [JsonPropertyName("models")]
    public List<RunningModelInfoDto>? Models { get; set; }
}

public sealed class OllamaVersionResponseDto
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}
