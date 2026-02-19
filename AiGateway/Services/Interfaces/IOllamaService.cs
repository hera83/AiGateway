using AiGateway.Dtos;

namespace AiGateway.Services.Interfaces;

public interface IOllamaService
{
    // Generate
    Task<GenerateResponseDto> GenerateAsync(GenerateRequestDto request, CancellationToken ct = default);
    IAsyncEnumerable<GenerateStreamChunkResponseDto> GenerateStreamAsync(GenerateRequestDto request, CancellationToken ct = default);

    // Chat
    Task<ChatResponseDto> ChatAsync(ChatRequestDto request, CancellationToken ct = default);
    IAsyncEnumerable<ChatStreamChunkResponseDto> ChatStreamAsync(ChatRequestDto request, CancellationToken ct = default);

    // Embeddings - new /api/embed endpoint
    Task<EmbedResponseDto> EmbedAsync(EmbedRequestDto request, CancellationToken ct = default);

    // Embeddings - deprecated /api/embeddings endpoint
    Task<EmbeddingsResponseDto> EmbeddingsAsync(EmbeddingsRequestDto request, CancellationToken ct = default);

    // Model management
    Task<ListModelsResponseDto> ListModelsAsync(CancellationToken ct = default);
    Task<ShowModelResponseDto> ShowModelAsync(ShowModelRequestDto request, CancellationToken ct = default);
    Task<PullModelResponseDto> PullModelAsync(PullModelRequestDto request, CancellationToken ct = default);
    IAsyncEnumerable<PullModelResponseDto> PullModelStreamAsync(PullModelRequestDto request, CancellationToken ct = default);
    Task DeleteModelAsync(DeleteModelRequestDto request, CancellationToken ct = default);
    Task CopyModelAsync(CopyModelRequestDto request, CancellationToken ct = default);
    Task<CreateModelResponseDto> CreateModelAsync(CreateModelRequestDto request, CancellationToken ct = default);
    IAsyncEnumerable<CreateModelResponseDto> CreateModelStreamAsync(CreateModelRequestDto request, CancellationToken ct = default);
}
