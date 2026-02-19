namespace AiGateway.Endpoints;

/// <summary>
/// Legacy wildcard proxy endpoints have been removed in favor of explicit actions.
/// </summary>
public static class ProxyEndpoints
{
    public static void MapProxyEndpoints(this WebApplication app)
    {
        // Intentionally left blank. Use SpeachesEndpoints and OllamaEndpoints instead.
    }
}
