# AiGateway - API Gateway & Proxy

A production-ready .NET 10 Web API gateway with transparent request forwarding, authentication, and API key management. Designed to proxy requests to multiple upstream services while enforcing security policies.

## Features

- **Transparent Proxy**: Forward requests to upstream services (Speaches and Ollama) with full support for GET/POST/PUT/PATCH/DELETE
- **Streaming Support**: Handle streaming responses without buffering; ideal for long-running operations
- **API Key Authentication**: x-api-key header-based authentication with master key and client key separation
- **SQLite Database**: File-based API key storage with hashed keys and per-key salts
- **Role-Based Access Control**: Master keys for administration, client keys for service access
- **Swagger UI**: Built-in API documentation with x-api-key header support for testing
- **Comprehensive Logging**: Serilog with console and daily rolling file logs
- **Error Handling**: Consistent ErrorDto responses across all endpoints
- **EF Core Integration**: Automatic database initialization and schema management

## Architecture

### Components

- **ApiKeyAuthenticationHandler**: Custom authentication handler validating x-api-key header
- **ApiKeyDbContext**: Entity Framework Core context for API key storage
- **ClientKeyService**: Business logic for key management (CRUD, validation)
- **HashingService**: SHA256 hashing with per-key random 16-byte salt
- **Endpoints**: Minimal API endpoints for key management and request proxying
- **Middleware**: Error handling and global exception catching

### Upstream Services

- **Speaches**: `http://10.64.10.4`
- **Ollama**: `http://10.64.10.5`

## Getting Started

### Prerequisites

- .NET 10 SDK
- Visual Studio Code or similar editor

### Installation & Setup

1. **Navigate to project**:
   ```bash
   cd AiGateway
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure settings** (optional):
   Edit `appsettings.json` to customize:
   - `Gateway:MasterKey` - UUID for master key (default: `12345678-1234-1234-1234-123456789012`)
   - `Gateway:DatabasePath` - SQLite database file location
   - `Upstreams:SpeachesBaseUrl` - Speaches service URL
   - `Upstreams:OllamaBaseUrl` - Ollama service URL

4. **Run the application**:
   ```bash
   dotnet run
   ```

   The gateway will start on `https://localhost:7001` (default HTTPS port in .NET 10)

5. **Access Swagger UI**:
   Open browser to: `https://localhost:7001/swagger`

## API Key Management

### Master Key Usage

The master key is used exclusively for administrative operations. In Swagger:

1. Click the "Authorize" button (lock icon)
2. Enter the master key as `x-api-key` header value (default: `12345678-1234-1234-1234-123456789012`)
3. Click "Authorize"

### Creating a Client Key

**Request**:
```bash
POST /v1/keys
x-api-key: 12345678-1234-1234-1234-123456789012
Content-Type: application/json

{
  "appName": "MyApp",
  "appContact": "contact@example.com",
  "appNote": "Optional notes about this key"
}
```

**Response** (201 Created):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "apiKey": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "appName": "MyApp",
  "appContact": "contact@example.com",
  "appNote": "Optional notes about this key"
}
```

**Important**: The plaintext `apiKey` is returned **only once** at creation. Store it securely.

### Listing Client Keys

```bash
GET /v1/keys
x-api-key: 12345678-1234-1234-1234-123456789012
```

### Getting a Specific Client Key

```bash
GET /v1/keys/{id}
x-api-key: 12345678-1234-1234-1234-123456789012
```

### Updating a Client Key

```bash
PUT /v1/keys/{id}
x-api-key: 12345678-1234-1234-1234-123456789012
Content-Type: application/json

{
  "appName": "UpdatedAppName",
  "appContact": "newemail@example.com",
  "enabled": true
}
```

### Rotating a Client Key

Generates a new plaintext key for an existing key ID:

```bash
POST /v1/keys/{id}/rotate
x-api-key: 12345678-1234-1234-1234-123456789012
```

**Response**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "apiKey": "new-plaintext-key-guid"
}
```

### Deleting a Client Key

```bash
DELETE /v1/keys/{id}
x-api-key: 12345678-1234-1234-1234-123456789012
```

## Proxy Endpoints

Client keys have full access to both upstream services through the proxy endpoints. Master keys are **blocked** from proxy access.

### Format

```
/v1/proxy/{service}/{**path}
```

Where `{service}` is either `speaches` or `ollama`.

### Example 1: Call Speaches Service

**Forward a GET request**:
```bash
GET /v1/proxy/speaches/api/v1/speeches
x-api-key: f47ac10b-58cc-4372-a567-0e02b2c3d479
```

This forwards to: `http://10.64.10.4/api/v1/speeches`

### Example 2: Call Ollama Service with POST

**Forward a POST request with body**:
```bash
POST /v1/proxy/ollama/api/generate
x-api-key: f47ac10b-58cc-4372-a567-0e02b2c3d479
Content-Type: application/json

{
  "model": "llama2",
  "prompt": "Why is the sky blue?"
}
```

This forwards to: `http://10.64.10.5/api/generate/`

### Query String Forwarding

Query strings are automatically forwarded:

```bash
GET /v1/proxy/speaches/api/v1/speeches?limit=10&offset=0
```

Forwards to: `http://10.64.10.4/api/v1/speeches?limit=10&offset=0`

### Streaming Responses

The proxy supports streaming responses without buffering:
- Request bodies are streamed to upstream
- Response bodies are streamed back to client
- Ideal for Ollama endpoints returning streaming text

## Security

### Authentication & Authorization

- **Authentication**: x-api-key header validation
- **Master Key Claims**: `is_master=true` (admin access only)
- **Client Key Claims**: `is_master=false`, `key_id=<UUID>`, `app_name=<string>`

### Key Storage

- Plaintext keys are **never** stored
- Each key is hashed with SHA256(salt + plaintext)
- Random 16-byte salt per key
- Salt stored alongside hash for verification

### Authorization Policies

- **MasterOnly**: `/v1/keys/*` endpoints restricted to master key only
- **ClientOnly**: `/v1/proxy/*` endpoints restricted to client keys (master blocked)
- **Authenticated**: All protected endpoints require valid x-api-key

### Headers Handling

**Forwarded Headers**:
- All standard headers (Content-Type, Accept, Authorization, etc.)
- Custom headers (X-Request-ID, X-Correlation-ID, etc.)

**Removed Headers** (hop-by-hop):
- Connection, Transfer-Encoding, Keep-Alive
- Proxy-Authenticate, Proxy-Authorization
- TE, Trailer, Upgrade

**Always Removed**:
- x-api-key (gateway authentication, not forwarded to upstream)
- Host (set by HttpClient based on upstream URL)

## Logging

Logs are written to console and daily rolling files in the `logs/` directory.

### Log Information

Each proxy request logs:
- HTTP method and path
- Upstream service and URL
- Response status code
- Request duration
- Client app name and key ID
- Whether master key was used

### Example Log Entry

```
2026-02-18 10:15:30.123 [INF] Proxy request: POST ollama/api/generate -> http://10.64.10.5/api/generate [200] - App: MyApp, KeyId: 550e8400-e29b-41d4-a716-446655440000
2026-02-18 10:15:30.456 [INF] Client key created: 550e8400-e29b-41d4-a716-446655440000 for app MyApp
```

### Log Retention

- Logs are retained for **31 days**
- One file per day (named `log-YYYYMMDD.txt`)
- Old logs are automatically deleted

## Database

The SQLite database stores:

### ClientKeyEntity Schema

| Column | Type | Notes |
|--------|------|-------|
| Id | GUID | Primary Key |
| KeyHash | Blob | SHA256(salt + plaintext_key) |
| Salt | Blob | Random 16-byte salt |
| AppName | Text | Required, max 255 chars |
| AppContact | Text | Required, max 255 chars |
| AppNote | Text | Optional, max 1000 chars |
| Enabled | Boolean | Default: true |
| CreatedAtUtc | DateTime | Set on creation |
| UpdatedAtUtc | DateTime | Updated on modification |
| LastUsedAtUtc | DateTime | Updated on successful auth |

## Error Responses

All errors return ErrorDto with consistent structure:

```json
{
  "code": "ERROR_CODE",
  "message": "Human-readable error message",
  "traceId": "0HN1GH7F5V4J7:00000001"
}
```

### Common Error Codes

- `BAD_REQUEST` (400): Validation failure
- `UNAUTHORIZED` (401): Missing or invalid x-api-key
- `FORBIDDEN` (403): Access policy violation (master accessing proxy, client accessing /keys)
- `NOT_FOUND` (404): Resource not found
- `BAD_GATEWAY` (502): Upstream connection failure
- `INTERNAL_ERROR` (500): Unexpected server error

## Development

### Project Structure

```
AiGateway/
├── Data/
│   ├── ApiKeyDbContext.cs         # EF Core DbContext
│   └── ClientKeyEntity.cs         # Database model
├── Dtos/
│   └── Dtos.cs                    # Request/response DTOs
├── Endpoints/
│   ├── KeyManagementEndpoints.cs  # /v1/keys/* routes
│   └── ProxyEndpoints.cs          # /v1/proxy/* routes
├── Middleware/
│   ├── ApiKeyAuthenticationHandler.cs  # x-api-key validation
│   └── ErrorHandlingMiddleware.cs      # Global error handling
├── Services/
│   ├── ClientKeyService.cs        # Key management logic
│   └── HashingService.cs          # SHA256 hashing
├── Program.cs                      # Main entry point
├── appsettings.json               # Configuration
├── AiGateway.csproj               # Project file
└── README.md                      # This file
```

### Building from Source

```bash
dotnet build
dotnet run
```

### Running Tests

Currently, no test project included. To add:
```bash
dotnet new xunit -n AiGateway.Tests
dotnet sln add AiGateway.Tests/AiGateway.Tests.csproj
```

## Performance Considerations

- **Streaming**: Responses are streamed without buffering to handle large payloads
- **HttpClient Timeout**: Set to 30 minutes for long-running operations
- **Connection Pooling**: HttpClient reuses connections via HttpClientHandler
- **Async/Await**: All I/O operations are non-blocking

## Troubleshooting

### Database File Not Found

**Issue**: "Cannot open database file"

**Solution**: Ensure the `data/` directory exists. The application creates it automatically, but check permissions.

```bash
mkdir data
```

### Master Key Not Working

**Issue**: Master key returns 401 Unauthorized

**Solution**: Verify the exact master key value in `appsettings.json` matches what you're sending.

```bash
dotnet user-secrets set "Gateway:MasterKey" "your-master-key-guid"
```

### Upstream Service Unreachable

**Issue**: Proxy requests return 502 Bad Gateway

**Solution**: Verify upstream service URLs are correct and services are running:

```bash
# Test connectivity
curl http://10.64.10.4/health
curl http://10.64.10.5/health
```

## Deployment

### Docker

To containerize:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet build "AiGateway.csproj"
RUN dotnet publish "AiGateway.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80 443
ENTRYPOINT ["dotnet", "AiGateway.dll"]
```

### Environment Variables

Set via environment variables (override appsettings.json):

```bash
export Gateway__MasterKey="your-master-key"
export Gateway__DatabasePath="/app/data/apikeys.sqlite"
export Upstreams__SpeachesBaseUrl="http://speaches:80"
export Upstreams__OllamaBaseUrl="http://ollama:80"
```

## Support & Contributing

For issues or suggestions, refer to internal documentation or contact the development team.

---

**Version**: 1.0.0  
**Framework**: .NET 10  
**Author**: AI Architect  
**Last Updated**: 2026-02-18
