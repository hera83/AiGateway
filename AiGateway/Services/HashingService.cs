using System.Security.Cryptography;
using System.Text;

namespace AiGateway.Services;

/// <summary>
/// Service for hashing and verifying API keys with per-key random salt.
/// </summary>
public interface IHashingService
{
    /// <summary>
    /// Hash a plaintext key with a randomly generated 16-byte salt.
    /// Returns tuple of (hash, salt).
    /// </summary>
    (byte[] hash, byte[] salt) HashKey(string plaintextKey);

    /// <summary>
    /// Verify a plaintext key against a stored hash and salt.
    /// </summary>
    bool VerifyKey(string plaintextKey, byte[] storedHash, byte[] storedSalt);
}

public class HashingService : IHashingService
{
    private const int SaltLength = 16; // 16 bytes

    public (byte[] hash, byte[] salt) HashKey(string plaintextKey)
    {
        // Generate random salt
        var salt = new byte[SaltLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash: SHA256(salt + utf8(key))
        var keyBytes = Encoding.UTF8.GetBytes(plaintextKey);
        var combined = new byte[salt.Length + keyBytes.Length];
        Array.Copy(salt, combined, salt.Length);
        Array.Copy(keyBytes, 0, combined, salt.Length, keyBytes.Length);

        byte[] hash;
        using (var sha256 = SHA256.Create())
        {
            hash = sha256.ComputeHash(combined);
        }

        return (hash, salt);
    }

    public bool VerifyKey(string plaintextKey, byte[] storedHash, byte[] storedSalt)
    {
        var keyBytes = Encoding.UTF8.GetBytes(plaintextKey);
        var combined = new byte[storedSalt.Length + keyBytes.Length];
        Array.Copy(storedSalt, combined, storedSalt.Length);
        Array.Copy(keyBytes, 0, combined, storedSalt.Length, keyBytes.Length);

        byte[] computedHash;
        using (var sha256 = SHA256.Create())
        {
            computedHash = sha256.ComputeHash(combined);
        }

        return computedHash.SequenceEqual(storedHash);
    }
}
