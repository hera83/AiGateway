using AiGateway.Data;
using AiGateway.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AiGateway.Services;

/// <summary>
/// Service for managing client API keys.
/// </summary>
public interface IClientKeyService
{
    Task<ClientKeyEntity?> GetKeyByIdAsync(Guid id);
    Task<ClientKeyEntity?> ValidateAndGetKeyAsync(string plaintextKey);
    Task<ClientKeyEntity> CreateKeyAsync(CreateClientKeyRequestDto request, string plaintextKey);
    Task<IEnumerable<ClientKeyDto>> ListKeysAsync();
    Task<ClientKeyEntity?> UpdateKeyAsync(Guid id, UpdateClientKeyRequestDto request);
    Task<ClientKeyEntity?> RotateKeyAsync(Guid id, string newPlaintextKey);
    Task<bool> DeleteKeyAsync(Guid id);
    Task UpdateLastUsedAsync(Guid id);
}

public class ClientKeyService : IClientKeyService
{
    private readonly ApiKeyDbContext _dbContext;
    private readonly IHashingService _hashingService;

    public ClientKeyService(ApiKeyDbContext dbContext, IHashingService hashingService)
    {
        _dbContext = dbContext;
        _hashingService = hashingService;
    }

    public async Task<ClientKeyEntity?> GetKeyByIdAsync(Guid id)
    {
        return await _dbContext.ClientKeys.FindAsync(id);
    }

    public async Task<ClientKeyEntity?> ValidateAndGetKeyAsync(string plaintextKey)
    {
        var allKeys = await _dbContext.ClientKeys
            .AsNoTracking()
            .Where(k => k.Enabled)
            .ToListAsync();

        foreach (var keyEntity in allKeys)
        {
            if (_hashingService.VerifyKey(plaintextKey, keyEntity.KeyHash, keyEntity.Salt))
            {
                return keyEntity;
            }
        }

        return null;
    }

    public async Task<ClientKeyEntity> CreateKeyAsync(CreateClientKeyRequestDto request, string plaintextKey)
    {
        var (hash, salt) = _hashingService.HashKey(plaintextKey);
        var entity = new ClientKeyEntity
        {
            Id = Guid.NewGuid(),
            KeyHash = hash,
            Salt = salt,
            AppName = request.AppName,
            AppContact = request.AppContact,
            AppNote = request.AppNote,
            Enabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.ClientKeys.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<ClientKeyDto>> ListKeysAsync()
    {
        return await _dbContext.ClientKeys
            .AsNoTracking()
            .Select(e => new ClientKeyDto
            {
                Id = e.Id,
                AppName = e.AppName,
                AppContact = e.AppContact,
                AppNote = e.AppNote,
                Enabled = e.Enabled,
                CreatedAtUtc = e.CreatedAtUtc,
                UpdatedAtUtc = e.UpdatedAtUtc,
                LastUsedAtUtc = e.LastUsedAtUtc
            })
            .ToListAsync();
    }

    public async Task<ClientKeyEntity?> UpdateKeyAsync(Guid id, UpdateClientKeyRequestDto request)
    {
        var entity = await _dbContext.ClientKeys.FindAsync(id);
        if (entity == null)
            return null;

        if (!string.IsNullOrEmpty(request.AppName))
            entity.AppName = request.AppName;

        if (!string.IsNullOrEmpty(request.AppContact))
            entity.AppContact = request.AppContact;

        if (request.AppNote != null)
            entity.AppNote = request.AppNote;

        if (request.Enabled.HasValue)
            entity.Enabled = request.Enabled.Value;

        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<ClientKeyEntity?> RotateKeyAsync(Guid id, string newPlaintextKey)
    {
        var entity = await _dbContext.ClientKeys.FindAsync(id);
        if (entity == null)
            return null;

        var (newHash, newSalt) = _hashingService.HashKey(newPlaintextKey);
        entity.KeyHash = newHash;
        entity.Salt = newSalt;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteKeyAsync(Guid id)
    {
        var entity = await _dbContext.ClientKeys.FindAsync(id);
        if (entity == null)
            return false;

        _dbContext.ClientKeys.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task UpdateLastUsedAsync(Guid id)
    {
        var entity = await _dbContext.ClientKeys.FindAsync(id);
        if (entity != null)
        {
            entity.LastUsedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }
}
