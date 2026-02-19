namespace AiGateway.Services;

public sealed class OllamaOptions
{
    public string BaseUrl { get; set; } = "http://127.0.0.1:11434";
    public string DefaultModel { get; set; } = "llama3.2";
    public int RequestTimeoutSeconds { get; set; } = 120;
    public string? DefaultKeepAlive { get; set; } = null;
}
