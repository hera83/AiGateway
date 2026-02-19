namespace AiGateway.Data;

/// <summary>
/// Represents a stored client API key with hashed value and metadata.
/// </summary>
public class ClientKeyEntity
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// SHA256 hash of (salt + plaintext key). Hashing is done with salt prepended.
    /// </summary>
    public byte[] KeyHash { get; set; } = null!;
    
    /// <summary>
    /// Random 16-byte salt used for hashing this key.
    /// </summary>
    public byte[] Salt { get; set; } = null!;
    
    public string AppName { get; set; } = null!;
    public string AppContact { get; set; } = null!;
    public string? AppNote { get; set; }
    
    public bool Enabled { get; set; } = true;
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? LastUsedAtUtc { get; set; }
}
