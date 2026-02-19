namespace AiGateway.Services;

public sealed class OllamaApiException : Exception
{
    public int StatusCode { get; }
    public string? ResponseBody { get; }

    public OllamaApiException(string message, int statusCode, string? responseBody)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
