# Copilot Custom Instructions

This file provides workspace-specific custom instructions for GitHub Copilot.

## Project Overview

**AiGateway** is a production-ready .NET 10 Web API gateway with:
- Transparent request proxying to upstream services (Speaches @ http://10.64.10.4, Ollama @ http://10.64.10.5)
- x-api-key authentication with master and client key separation
- SQLite-persisted API key management with SHA256 hashing
- Role-based authorization policies (MasterOnly for admin, ClientOnly for proxy)
- Comprehensive error handling and Serilog logging
- Swagger/OpenAPI documentation with x-api-key integration
- Full streaming support for long-running requests

## Architecture Guidelines

### Key Components
- **Authentication**: Custom `ApiKeyAuthenticationHandler` validates x-api-key header against master key and hashed client keys
- **Database**: EF Core with SQLite; `ClientKeyEntity` stores hashed keys with per-key 16-byte random salt
- **Endpoints**: Minimal APIs using MapGroup for clean routing (v1/keys/* for admin, v1/proxy/* for client proxying)
- **Security**: Master key (admin) and client keys (service access) are mutually exclusive

### Authentication Flow
1. Client sends x-api-key header in request
2. ApiKeyAuthenticationHandler intercepts and validates:
   - If matches MasterKey → claims: is_master=true
   - If validates against hashed client key → claims: is_master=false + key_id + app_name
3. Authorization policies enforce role separation via claims

### Hashing Strategy
- Always use SHA256(salt + utf8(key)) where salt is random 16-byte blob
- Per-key random salt prevents rainbow table attacks
- Salt and hash both stored in database; plaintext never stored or logged

## Development Rules

### Code Style
- Use Minimal APIs (MapGroup, MapPost, etc.) over controller-based routing
- Implement error handling consistently via ErrorDto and middleware
- Use DTOs for all request/response types, even for Swagger documentation
- Don't log plaintext API keys; use key_id or app_name instead

### Database & Migrations
- Use EF Core with `Database.EnsureCreated()` at startup for initialization
- Use entity configuration in OnModelCreating for constraints and defaults
- Store blobs (salt, hash) as byte[] properties

### API Key Management
- Client key creation: Generate random GUID as plaintext, hash with new salt, return plaintext once only
- Client key validation: Load all keys, verify each against provided plaintext (time-safe comparison)
- Client key rotation: Generate new GUID, new salt, update hash, return new plaintext

### Proxy Forwarding
- Remove hop-by-hop headers (Connection, Transfer-Encoding, etc.) and x-api-key before forwarding
- Stream responses without buffering using HttpCompletionOption.ResponseHeadersRead
- Preserve Content-Type, Accept, and standard headers
- Copy response status code and headers back to client
- Handle timeouts gracefully with HttpClient timeout set to 30 minutes

### Logging
- Use Serilog with structured properties
- Log proxy requests: method, service, path, upstream URL, status, duration, app_name, key_id
- Log key operations: creation, update, rotation, deletion
- Don't log plaintext keys; use identifiers instead
- File logs: daily rolling with 31-day retention

### Authorization
- MasterOnly policy: Block ALL proxy/service access; master key for administration only
- ClientOnly policy: Block access to /v1/keys/*; client keys for proxy/service access only
- Enforce via policy checks on MapGroup and endpoint definitions

## Common Tasks

### Adding a New Endpoint
1. Create endpoint method that takes HttpContext, required services as parameters
2. Map using app.MapPost/Get/Put/Delete with .RequireAuthorization("PolicyName")
3. Return results using Results.Ok(), Results.BadRequest(), etc.
4. Add .Produces() for Swagger documentation
5. Log significant operations using Serilog (don't log keys)

### Updating Upstream URLs
1. Modify `appsettings.json` under `Upstreams` section
2. Restart application (no code change needed)
3. Test via Swagger with client key

### Creating Database Migrations
1. Add new property to ClientKeyEntity or create new entity class
2. Run: `dotnet ef migrations add MigrationName`
3. Run: `dotnet ef database update`
4. Commit migration files to repository

### Debugging Authentication Issues
1. Check x-api-key header value exactly matches configuration
2. Verify key is enabled in database (Enabled column)
3. Check logs for validation errors
4. Use Swagger "Authorize" button to set x-api-key header for testing

## Files Overview

- **Program.cs**: Dependency injection, middleware setup, endpoint mapping, database initialization
- **ApiKeyDbContext.cs**: EF Core configuration, entity mappings
- **ClientKeyService.cs**: Business logic for CRUD operations on client keys
- **HashingService.cs**: SHA256 hashing with salt generation and verification
- **ApiKeyAuthenticationHandler.cs**: custom authentication scheme for x-api-key validation
- **KeyManagementEndpoints.cs**: POST/GET/PUT/DELETE routes for key administration (MasterOnly)
- **ProxyEndpoints.cs**: Catch-all route /v1/proxy/{service}/{**path} with streaming (ClientOnly)
- **Dtos.cs**: All request/response DTOs for API contracts
- **ErrorHandlingMiddleware.cs**: Global exception handler returning ErrorDto
- **README.md**: Comprehensive documentation

## Testing Guidelines

### Manual Testing with curl
```bash
# Create key (use master key)
curl -X POST https://localhost:7001/v1/keys \
  -H "x-api-key: 12345678-1234-1234-1234-123456789012" \
  -H "Content-Type: application/json" \
  -d '{"appName":"Test","appContact":"test@example.com"}'

# Use returned apiKey to call proxy
curl -X GET https://localhost:7001/v1/proxy/speaches/api/v1/data \
  -H "x-api-key: <returned-api-key>"
```

### Swagger Testing
1. Navigate to https://localhost:7001/swagger
2. Click "Authorize" (lock icon)
3. Enter x-api-key header value
4. Test endpoints directly in Swagger UI

## Performance Notes

- HttpClient uses connection pooling automatically
- Responses streamed without buffering for large payloads
- LastUsedAtUtc updated on every successful client key auth (async SaveChanges)
- Avoid N+1 queries: Proxy endpoints don't query database per request

## Security Reminders

- Master key should be strong and rotated regularly
- Client keys are returned plaintext only at creation—store securely
- Disable client keys instead of deleting them for audit trail
- Monitor LastUsedAtUtc to detect inactive or compromised keys
- x-api-key is removed before forwarding to upstream (no leakage)
- All errors return generic public messages; detailed info in logs only

## Future Enhancements

- Add rate limiting per client key
- Implement request signing/validation
- Add webhook callbacks for key lifecycle events
- Support OAuth2/JWT in addition to API keys
- Add audit logging for compliance
- Implement circuit breaker pattern for upstream failures
