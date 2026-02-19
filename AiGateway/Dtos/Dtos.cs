namespace AiGateway.Dtos;

/// <summary>
/// Error response DTO returned by the gateway for all failure scenarios.
/// </summary>
public class ErrorDto
{
    public string Code { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string TraceId { get; set; } = null!;
    public object? Details { get; set; }
}

/// <summary>
/// Request DTO for creating a new client API key.
/// </summary>
public class CreateClientKeyRequestDto
{
    public string AppName { get; set; } = null!;
    public string AppContact { get; set; } = null!;
    public string? AppNote { get; set; }
}

/// <summary>
/// Response DTO for key creation - includes the plaintext API key (returned only once).
/// </summary>
public class CreateClientKeyResponseDto
{
    public Guid Id { get; set; }
    public string ApiKey { get; set; } = null!;
    public string AppName { get; set; } = null!;
    public string AppContact { get; set; } = null!;
    public string? AppNote { get; set; }
}

/// <summary>
/// DTO for listing/getting client key information (no plaintext key returned).
/// </summary>
public class ClientKeyDto
{
    public Guid Id { get; set; }
    public string AppName { get; set; } = null!;
    public string AppContact { get; set; } = null!;
    public string? AppNote { get; set; }
    public bool Enabled { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? LastUsedAtUtc { get; set; }
}

/// <summary>
/// Request DTO for updating an existing client key.
/// </summary>
public class UpdateClientKeyRequestDto
{
    public string? AppName { get; set; }
    public string? AppContact { get; set; }
    public string? AppNote { get; set; }
    public bool? Enabled { get; set; }
}

/// <summary>
/// Response DTO for key rotation - includes the new plaintext API key.
/// </summary>
public class RotateClientKeyResponseDto
{
    public Guid Id { get; set; }
    public string ApiKey { get; set; } = null!;
}

/// <summary>
/// Request DTO for proxy endpoints (documentation only, actual implementation forwards raw HTTP).
/// </summary>
public class ProxyRequestDto
{
    public string Service { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string Method { get; set; } = null!;
    public string? QueryString { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? BodyBase64 { get; set; }
}

/// <summary>
/// Response DTO for proxy endpoints (documentation only, actual implementation forwards raw HTTP).
/// </summary>
public class ProxyResponseDto
{
    public int StatusCode { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? BodyBase64 { get; set; }
}
