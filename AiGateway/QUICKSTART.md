# AiGateway - Quick Start Guide

## 📦 Project Generated Successfully

Your complete .NET 10 API Gateway project has been created with all production-ready features included.

## 📂 Project Structure

```
AiGateway/
├── .github/
│   └── copilot-instructions.md          # Architecture & development guidelines
├── Data/
│   ├── ApiKeyDbContext.cs               # EF Core DbContext
│   └── ClientKeyEntity.cs               # Database model for API keys
├── Dtos/
│   └── Dtos.cs                          # All request/response DTOs
├── Endpoints/
│   ├── KeyManagementEndpoints.cs        # /v1/keys/* admin endpoints
│   └── ProxyEndpoints.cs                # /v1/proxy/* forwarding routes
├── Middleware/
│   ├── ApiKeyAuthenticationHandler.cs   # Custom x-api-key auth
│   └── ErrorHandlingMiddleware.cs       # Global error handling
├── Services/
│   ├── ClientKeyService.cs              # Key management business logic
│   └── HashingService.cs                # SHA256 hashing with salt
├── Migrations/                          # (Empty, ready for EF migrations)
├── Program.cs                           # Application entry point
├── AiGateway.csproj                     # Project file with dependencies
├── appsettings.json                     # Configuration (MasterKey, DB path, upstream URLs)
├── appsettings.Development.json         # Development logging settings
├── README.md                            # Comprehensive documentation
└── .gitignore                           # Git ignore patterns
```

## 🚀 Get Started in 3 Steps

### 1. Restore & Build
```bash
cd AiGateway
dotnet restore
dotnet build
```

### 2. Run the Application
```bash
dotnet run
```

The server starts at:
- **HTTPS**: `https://localhost:7001`
- **HTTP**: `http://localhost:5000`

### 3. Open Swagger UI
Navigate to: **`https://localhost:7001/swagger/ui`**

## 🔑 Authentication Quick Test

### Get Master Key
Default master key (from `appsettings.json`):
```
12345678-1234-1234-1234-123456789012
```

### Test in Swagger
1. Click the **Authorize** button (🔒 icon)
2. Paste the master key in the header field
3. Click **Authorize**

### Create Your First Client Key
Send a POST request in Swagger:
```json
POST /v1/keys
{
  "appName": "MyFirstApp",
  "appContact": "dev@example.com",
  "appNote": "Testing the gateway"
}
```

**Response** (save the `apiKey`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "apiKey": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "appName": "MyFirstApp",
  "appContact": "dev@example.com",
  "appNote": "Testing the gateway"
}
```

## 🌐 Test Proxy Forwarding

With your new client `apiKey`, test proxying to an upstream service:

```bash
# From command line:
curl -X GET https://localhost:7001/v1/proxy/speaches/api/health \
  -H "x-api-key: f47ac10b-58cc-4372-a567-0e02b2c3d479" \
  -H "Content-Type: application/json"
```

Or in Swagger:
1. Authorize with the **client key** (not master key)
2. Go to `/v1/proxy` endpoints
3. Enter `service=speaches` and `path=api/health`
4. Execute

## ⚙️ Configuration

Edit `appsettings.json` to customize:

```json
{
  "Gateway": {
    "MasterKey": "your-master-key-uuid",
    "DatabasePath": "data/apikeys.sqlite"
  },
  "Upstreams": {
    "SpeachesBaseUrl": "http://10.64.10.4",
    "OllamaBaseUrl": "http://10.64.10.5"
  }
}
```

## 📚 Database

- **Type**: SQLite (file-based)
- **Location**: `data/apikeys.sqlite` (created automatically)
- **Schema**: Auto-created on first run
- **API Keys**: Stored hashed with per-key random salt (SHA256)

## 🔐 Security Features

✅ Master key for admin operations only  
✅ Client keys with isolated access to proxy endpoints  
✅ SHA256 hashing with 16-byte random salt per key  
✅ Plaintext keys returned **only once** at creation  
✅ Automatic `LastUsedAtUtc` tracking  
✅ Hop-by-hop header removal (no leakage to upstream)  
✅ x-api-key never forwarded upstream  

## 📊 Logging

Logs are written to:
- **Console**: Real-time output
- **Files**: `logs/log-YYYYMMDD.txt` (daily rolling, 31-day retention)

Sample log:
```
2026-02-18 10:15:30.123 [INF] Proxy request: GET speaches/api/health -> http://10.64.10.4/api/health [200] - App: MyFirstApp, KeyId: 550e8400-...
```

## 🔄 API Operations Cheat Sheet

### Master Key Only (Admin)
- `POST /v1/keys` - Create key
- `GET /v1/keys` - List all keys  
- `GET /v1/keys/{id}` - Get specific key metadata
- `PUT /v1/keys/{id}` - Update key settings
- `POST /v1/keys/{id}/rotate` - Rotate key (new plaintext)
- `DELETE /v1/keys/{id}` - Delete key

### Client Key Only (Service Access)
- `GET/POST/PUT/PATCH/DELETE /v1/proxy/{service}/{**path}` - Forward requests

### Public
- `GET /health` - Health check

## 🛠 Development Tasks

### Create a Migration
```bash
dotnet ef migrations add InitialCreate --context ApiKeyDbContext
dotnet ef database update
```

### Add New Upstream Service
1. Edit `appsettings.json` with new upstream URL
2. Update proxy endpoint to handle new service name
3. No code restart needed (configuration-driven)

### Debug Authentication
- Check `appsettings.json` for exact master key value
- Verify x-api-key header in request
- Check logs for validation errors
- Confirm key is enabled in database (Enabled=true)

## 📖 Full Documentation

See [README.md](README.md) for:
- Complete API endpoint reference  
- Error handling & response codes
- Deployment instructions
- Performance tuning  
- Troubleshooting guide

## ✔️ Verification Checklist

- [x] Project builds without errors
- [x] All dependencies resolved
- [x] Database schema auto-initializes
- [x] Authentication handler functional
- [x] Swagger documentation available
- [x] Logging configured (console + file)
- [x] Error handling middleware active
- [x] Streaming responses supported
- [x] Master & client key separation enforced

## 🚨 Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| **x-api-key not working** | Verify exact key value in appsettings.json and header |
| **Database file not found** | Application creates `data/` dir automatically; check write permissions |
| **Upstream returns 502** | Verify upstream URLs are correct and services are running |
| **Logs not appearing** | Check `logs/` directory; ensure write permissions |
| **Port already in use** | Change ports in `launchSettings.json` or use different port |

## 📝 Next Steps

1. **Configure upstream services**  
   Update `appsettings.json` with actual upstream URLs

2. **Change master key**  
   Generate new UUID and update `Gateway:MasterKey`

3. **Deploy to production**  
   Set environment variables for sensitive config

4. **Add custom business logic**  
   Extend endpoints or services as needed

---

**Ready to proxy?** Run `dotnet run` and navigate to `https://localhost:7001/swagger` 🎉
