# AiGateway - API Gateway & Proxy

A production-ready .NET 10 Web API gateway with transparent request forwarding, authentication, and API key management. Designed to proxy requests to multiple upstream services while enforcing security policies.

**Supports Docker deployment with persistent data and logs.**

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

## Docker (Recommended)

### Quick Start

```bash
mkdir -p data logs
cp .env.example .env

docker compose up -d --build
```

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`

### Permission Handling (Automatic)

The container starts as `root`, fixes ownership on `/data` and `/logs`, and then drops to the non-root app user using `gosu`. This prevents SQLite Error 14 without manual `chown`.

**Requirement:** host folders must exist before starting:
```bash
mkdir -p data logs
```

### Environment Variables (.env)

- `AIGATEWAY_MASTER_KEY` (required)
- `AIGATEWAY_UID` / `AIGATEWAY_GID` (optional, default 1000)
- `SPEACHES_BASE_URL`
- `OLLAMA_BASE_URL`
- `Gateway__ForceHttp11ForOllama` (optional: true/false)
- `Proxy__StripSensitiveHeaders` (optional: true/false, default true)

### Header Filtering

The gateway **automatically strips sensitive headers** before forwarding to upstream services to prevent auth leakage:

**Blocked Headers:**
- Authentication: `x-api-key`, `authorization`, `cookie`, `set-cookie`
- Proxying metadata: `x-forwarded-for`, `x-forwarded-proto`, `x-forwarded-host`, `forwarded`
- Infrastructure: `host`, `content-length`, hop-by-hop headers

**Debugging:** Set `Proxy__StripSensitiveHeaders=false` to disable filtering (development only).

### Diagnostics

- `GET /diag/ollama` (master key only)
  - Tests Ollama `/api/version`
  - Tests Ollama `/api/generate` (stream=false)
  - Returns status + response snippet

### Database Schema

The gateway uses **SQLite** with **EF Core migrations** for database schema management.

**Automatic Setup**: On first startup, the application automatically:
1. Creates the `/data` directory if missing
2. Runs pending EF Core migrations via `dbContext.Database.Migrate()`
3. Creates the `ClientKeys` table in SQLite

**Schema Location**: `/data/apikeys.sqlite` (inside container)

**No manual SQL required** - the schema is created automatically from migrations in `AiGateway/Migrations/`.

If you need to recreate the database:
```bash
docker compose down -v
rm -rf data/
docker compose up -d --build
```

---

## Security Model

### Authorization Policies

- **Master Key**: Can access `/v1/keys` (key management) and `/diag/*` (diagnostics)
  - **Cannot** access `/v1/ollama/*` or `/v1/speaches/*`
- **Client Key**: Can access `/v1/ollama/*` and `/v1/speaches/*` (AI services)
  - **Cannot** access `/v1/keys` or `/diag/*`
- **Public** (no auth needed): `/health`, `/swagger`

### Test Checklist

```bash
# 1. Health check (public, no auth)
curl http://localhost:8080/health
# Expected: 200 { "status": "healthy" }

# 2. Create key with master key (allowed)
curl -X POST http://localhost:8080/v1/keys/create \
  -H "x-api-key: 12345678-1234-1234-1234-123456789012" \
  -H "Content-Type: application/json" \
  -d '{"appName":"TestApp","appContact":"test@example.com"}'
# Expected: 201 with client key GUID

# 3. Create key with client key (forbidden)
curl -X POST http://localhost:8080/v1/keys/create \
  -H "x-api-key: <CLIENT_KEY_FROM_STEP_2>" \
  -H "Content-Type: application/json" \
  -d '{"appName":"Another","appContact":"another@example.com"}'
# Expected: 403 "Client keys cannot manage API keys"

# 4. Generate text with client key (allowed)
curl -X POST http://localhost:8080/v1/ollama/api/generate \
  -H "x-api-key: <CLIENT_KEY_FROM_STEP_2>" \
  -H "Content-Type: application/json" \
  -d '{"model":"llama3.2","prompt":"test","stream":false}'
# Expected: 200 (proxied to Ollama)

# 5. Generate text with master key (forbidden)
curl -X POST http://localhost:8080/v1/ollama/api/generate \
  -H "x-api-key: 12345678-1234-1234-1234-123456789012" \
  -H "Content-Type: application/json" \
  -d '{"model":"llama3.2","prompt":"test","stream":false}'
# Expected: 403 "Master keys cannot access Ollama endpoints"

# 6. Diagnostics with master key (allowed)
curl http://localhost:8080/diag/ollama \
  -H "x-api-key: 12345678-1234-1234-1234-123456789012"
# Expected: 200 with version + generate test results

# 7. Diagnostics with client key (forbidden)
curl http://localhost:8080/diag/ollama \
  -H "x-api-key: <CLIENT_KEY_FROM_STEP_2>"
# Expected: 403 "Client keys cannot access diagnostics"
```

## Logging

Startup logs include:
- Environment (Development/Production)
- Database path
- Upstream service URLs
- HTTP/1.1 forcing status (if configured)
- Auth policy summary

Example startup output:
```
[INF] ===== AiGateway Startup Summary =====
[INF] Environment: Production
[INF] Database: /data/apikeys.sqlite
[INF] Upstreams: Ollama=http://10.64.10.5:11434, Speaches=http://10.64.10.4:8000
[INF] Auth Policies: Master can access /v1/keys + /diag; Client can access /v1/ollama + /v1/speaches
[INF] Listening on: http://+:8080
[INF] =====================================
```

---

## Configuration
