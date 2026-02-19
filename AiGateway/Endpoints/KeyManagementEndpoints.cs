using AiGateway.Dtos;
using AiGateway.Middleware;
using AiGateway.Services;
using Serilog;

namespace AiGateway.Endpoints;

/// <summary>
/// Endpoints for managing client API keys (MasterOnly access).
/// </summary>
public static class KeyManagementEndpoints
{
    public static void MapKeyManagementEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/keys")
            .WithTags("Key Management")
            .RequireAuthorization("MasterOnly");

        group.MapPost("/", CreateClientKey)
            .WithName("CreateKey")
            .WithSummary("Create client key")
            .WithDescription("Creates a new client API key (returned once) with app metadata. Master key required.")
            .Produces<CreateClientKeyResponseDto>(StatusCodes.Status201Created)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden);

        group.MapGet("/", ListClientKeys)
            .WithName("ListKeys")
            .WithSummary("List client keys")
            .WithDescription("Lists all client keys without returning plaintext API keys. Master key required.")
            .Produces<List<ClientKeyDto>>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden);

        group.MapGet("/{id}", GetClientKey)
            .WithName("GetKey")
            .WithSummary("Get client key")
            .WithDescription("Gets a single client key by id (metadata only, no plaintext key). Master key required.")
            .Produces<ClientKeyDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound);

        group.MapPut("/{id}", UpdateClientKey)
            .WithName("UpdateKey")
            .WithSummary("Update client key")
            .WithDescription("Updates app metadata and/or enabled status for a client key. Master key required.")
            .Produces<ClientKeyDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound);

        group.MapPost("/{id}/rotate", RotateClientKey)
            .WithName("RotateKey")
            .WithSummary("Rotate client key")
            .WithDescription("Rotates a client key and returns a new plaintext API key once. Master key required.")
            .Produces<RotateClientKeyResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id}", DeleteClientKey)
            .WithName("DeleteKey")
            .WithSummary("Delete client key")
            .WithDescription("Deletes a client key permanently. Master key required.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateClientKey(
        CreateClientKeyRequestDto request,
        IClientKeyService keyService,
        HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.AppName) || string.IsNullOrWhiteSpace(request.AppContact))
        {
            return ErrorResponseWriter.ToResult(
                context,
                StatusCodes.Status400BadRequest,
                "BAD_REQUEST",
                "AppName and AppContact are required");
        }

        var plaintextKey = Guid.NewGuid().ToString();
        var createdEntity = await keyService.CreateKeyAsync(request, plaintextKey);

        var response = new CreateClientKeyResponseDto
        {
            Id = createdEntity.Id,
            ApiKey = plaintextKey,
            AppName = createdEntity.AppName,
            AppContact = createdEntity.AppContact,
            AppNote = createdEntity.AppNote
        };

        Log.Information("Client key created: {KeyId} for app {AppName}", createdEntity.Id, request.AppName);

        return Results.Created($"/v1/keys/{createdEntity.Id}", response);
    }

    private static async Task<IResult> ListClientKeys(
        IClientKeyService keyService,
        HttpContext context)
    {
        var keys = await keyService.ListKeysAsync();
        Log.Information("Client keys listed: {Count} keys", keys.Count());
        return Results.Ok(keys);
    }

    private static async Task<IResult> GetClientKey(
        Guid id,
        IClientKeyService keyService,
        HttpContext context)
    {
        var entity = await keyService.GetKeyByIdAsync(id);
        if (entity == null)
        {
            return ErrorResponseWriter.ToResult(
                context,
                StatusCodes.Status404NotFound,
                "NOT_FOUND",
                $"Client key {id} not found");
        }

        var response = new ClientKeyDto
        {
            Id = entity.Id,
            AppName = entity.AppName,
            AppContact = entity.AppContact,
            AppNote = entity.AppNote,
            Enabled = entity.Enabled,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            LastUsedAtUtc = entity.LastUsedAtUtc
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateClientKey(
        Guid id,
        UpdateClientKeyRequestDto request,
        IClientKeyService keyService,
        HttpContext context)
    {
        var entity = await keyService.UpdateKeyAsync(id, request);
        if (entity == null)
        {
            return ErrorResponseWriter.ToResult(
                context,
                StatusCodes.Status404NotFound,
                "NOT_FOUND",
                $"Client key {id} not found");
        }

        var response = new ClientKeyDto
        {
            Id = entity.Id,
            AppName = entity.AppName,
            AppContact = entity.AppContact,
            AppNote = entity.AppNote,
            Enabled = entity.Enabled,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            LastUsedAtUtc = entity.LastUsedAtUtc
        };

        Log.Information("Client key updated: {KeyId}", id);

        return Results.Ok(response);
    }

    private static async Task<IResult> RotateClientKey(
        Guid id,
        IClientKeyService keyService,
        HttpContext context)
    {
        var entity = await keyService.GetKeyByIdAsync(id);
        if (entity == null)
        {
            return ErrorResponseWriter.ToResult(
                context,
                StatusCodes.Status404NotFound,
                "NOT_FOUND",
                $"Client key {id} not found");
        }

        var newPlaintextKey = Guid.NewGuid().ToString();
        var rotatedEntity = await keyService.RotateKeyAsync(id, newPlaintextKey);

        var response = new RotateClientKeyResponseDto
        {
            Id = rotatedEntity!.Id,
            ApiKey = newPlaintextKey
        };

        Log.Information("Client key rotated: {KeyId}", id);

        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteClientKey(
        Guid id,
        IClientKeyService keyService,
        HttpContext context)
    {
        var deleted = await keyService.DeleteKeyAsync(id);
        if (!deleted)
        {
            return ErrorResponseWriter.ToResult(
                context,
                StatusCodes.Status404NotFound,
                "NOT_FOUND",
                $"Client key {id} not found");
        }

        Log.Information("Client key deleted: {KeyId}", id);

        return Results.StatusCode(StatusCodes.Status204NoContent);
    }
}
