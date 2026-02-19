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
- **🐳 Docker Support**: Multi-stage build with proper permission handling for persistent volumes

## Quick Start with Docker

### Prerequisites
- Docker & Docker Compose installed
- Access to Ollama and Speaches services

### Setup (Quick)

1. **Clone repository:**
   ```bash
   git clone https://github.com/hera83/AiGateway.git
   cd AiGateway
   ```

2. **Create required directories:**
   ```bash
   mkdir -p data logs
   ```

3. **Create environment file:**
   ```bash
   cp .env.example .env
   ```

4. **Set correct UID/GID (recommended):**
   
   On Linux/MacOS/WSL2:
   ```bash
   # Get your user ID and group ID
   id -u   # Your UID
   id -g   # Your GID
   
   # Update .env with your values
   sed -i "s/AIGATEWAY_UID=1000/AIGATEWAY_UID=$(id -u)/" .env
   sed -i "s/AIGATEWAY_GID=1000/AIGATEWAY_GID=$(id -g)/" .env
   ```
   
   On Windows (cmd):
   ```batch
   REM Windows users can usually leave defaults (1000) unless you get permission errors
   ```

5. **Start the gateway:**
   ```bash
   docker compose up -d --build
   ```

6. **Verify it's running:**
   ```bash
   docker compose logs -f aigateway
   ```
   
   You should see:
   ```
   AiGateway Entrypoint ===
   Running as: uid=0(root) gid=0(root) groups=0(root)
   Running as root - fixing permissions...
   Dropping to non-root user: appuser (1000:1000)
   ```
   
   After ~40 seconds, health check should turn green:
   ```
   docker compose ps
   # STATUS should be "Up X seconds (healthy)"
   ```
### Access the Gateway

- **Swagger UI:** http://localhost:8080/swagger
- **API Base:** http://localhost:8080
- **Health Check:** http://localhost:8080/health

### Persistent Data

- **SQLite Database:** `./data/apikeys.sqlite` (mounted from `/data` in container)
- **Logs:** `./logs/` (mounted from `/logs` in container)

Both directories are created automatically and persist between restarts.

### Common Docker Commands

```bash
# View logs
docker compose logs -f aigateway

# Stop the gateway
docker compose down

# Restart the gateway
docker compose restart

# Stop and remove all data volumes
docker compose down -v

# Rebuild image after code changes
docker compose up -d --build

# Run a command in the container
docker compose exec aigateway dotnet --version

# Check permissions inside container
docker compose exec aigateway id
docker compose exec aigateway ls -la /data
```

## Configuration

### Environment Variables (Docker)

Set these in `.env` when using Docker Compose:

| Variable | Default | Description |
|----------|---------|-------------|
| `AIGATEWAY_MASTER_KEY` | Required | GUID for internal security |
| `AIGATEWAY_UID` | 1000 | Container user ID (for volume permissions) |
| `AIGATEWAY_GID` | 1000 | Container group ID (for volume permissions) |
| `SPEACHES_BASE_URL` | http://10.64.10.4:8000 | Speaches service URL |
| `OLLAMA_BASE_URL` | http://10.64.10.5:11434 | Ollama service URL |
| `OLLAMA_DEFAULT_MODEL` | llama3.2 | Default Ollama model |
| `OLLAMA_TIMEOUT` | 120 | Request timeout in seconds |

### Configuration Files

- **appsettings.json** - Default configuration (overridable by env vars)
- **appsettings.Development.json** - Development-specific settings
- **.env.example** - Example environment variables for Docker

## Local Development (without Docker)

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

3. **Run the application**:
   ```bash
   dotnet run
   ```

The application will start on https://localhost:5001 and http://localhost:5000.

## Architecture

### Components

- **ApiKeyAuthenticationHandler**: Custom authentication handler validating x-api-key header
- **ApiKeyDbContext**: Entity Framework Core context for API key storage
- **ClientKeyService**: Business logic for key management (CRUD, validation)
- **HashingService**: SHA256 hashing with per-key random 16-byte salt
- **OllamaService**: Typed service for Ollama API calls with streaming support
- **Endpoints**: Minimal API endpoints for key management and request proxying
- **Middleware**: Error handling and global exception catching

### Docker Architecture & Permission Handling

The Docker setup uses a robust privilege-dropping mechanism:

1. **Build Phase** (Dockerfile):
   - Multi-stage build: SDK → Runtime (Debian-based aspnet:10.0)
   - Creates non-root user `appuser` with configurable UID/GID
   - Installs `gosu` for safe privilege dropping (more reliable than `su`)
   - Installs `curl` for healthcheck endpoint

2. **Entrypoint Script** (`entrypoint.sh`):
   - Always starts as `root` (required to fix volume permissions)
   - Ensures `/data` and `/logs` directories are writable by app user
   - Uses `gosu` to safely drop to app user before starting .NET app
   - Logs warnings if permissions are incorrect for debugging

3. **Runtime** (docker-compose.yml):
   - Passes UID/GID from `.env` to Dockerfile as build args
   - Sets container `user:` to match host user UID/GID
   - Mounts volumes: `./data:/data` and `./logs:/logs`
   - Healthcheck: `curl -f http://localhost:8080/health`

This design ensures:
- ✅ No `chmod 777` or security compromises needed
- ✅ Container runs non-root even after privilege drop
- ✅ Host volumes accessible regardless of ownership
- ✅ SQLite database readable/writable by both host and container

### Upstream Services

- **Speaches**: `http://10.64.10.4:8000`
- **Ollama**: `http://10.64.10.5:11434`

## API Endpoints

### Key Management
- `POST /api/keys/create` - Create new API key (master key required)
- `GET /api/keys` - List API keys (master key required)
- `DELETE /api/keys/{id}` - Delete API key (master key required)

### Ollama (Forwarded)
- `POST /api/ollama/generate` - Text generation
- `POST /api/ollama/chat` - Chat completion
- `POST /api/ollama/embed` - Generate embeddings
- `GET /api/ollama/models` - List models
- `POST /api/ollama/pull` - Download model
- `POST /api/ollama/delete` - Delete model

### Speaches (Forwarded)
- `POST /api/speaches/**` - All Speaches requests forwarded

### System
- `GET /health` - Health check
- `GET /swagger` - Swagger UI

## Troubleshooting

### SQLite Error 14: 'unable to open database file'

This happens when the container user cannot write to `/data` volume.

**Solution:**

1. **Check permissions on host:**
   ```bash
   ls -la data/
   ls -la logs/
   ```

2. **Check permissions inside container:**
   ```bash
   docker compose exec aigateway id
   docker compose exec aigateway ls -la /data
   docker compose exec aigateway ls -la /logs
   ```

3. **Fix permissions - choose one:**

   **Option A: Set UID/GID in .env (recommended)**
   ```bash
   # Get your UID and GID
   YOUR_UID=$(id -u)
   YOUR_GID=$(id -g)
   
   # Update .env
   sed -i "s/AIGATEWAY_UID=1000/AIGATEWAY_UID=$YOUR_UID/" .env
   sed -i "s/AIGATEWAY_GID=1000/AIGATEWAY_GID=$YOUR_GID/" .env
   
   # Rebuild and restart
   docker compose down -v
   docker compose up -d --build
   ```

   **Option B: Fix permissions on host (if UID/GID mismatch)**
   ```bash
   # Recursively fix permissions for existing data
   sudo chown -R 1000:1000 data/ logs/
   sudo chmod -R 755 data/ logs/
   ```

   **Option C: Re-initialize (nuclear option)**
   ```bash
   # Remove everything and start fresh
   docker compose down -v
   rm -rf data logs
   mkdir -p data logs
   docker compose up -d --build
   ```

### Container exits or logs show permission denied

```bash
# 1. Check what UID/GID the container is trying to use
docker compose exec aigateway id

# 2. Compare with host directories
ls -la data/ logs/

# 3. If mismatch, update .env with correct AIGATEWAY_UID/AIGATEWAY_GID
# and rebuild the container
docker compose down
docker compose up -d --build
```

### Health check fails

```bash
# Check if /health endpoint responds
curl -v http://localhost:8080/health

# Check container logs
docker compose logs -f aigateway | grep -i health

# Verify application started
docker compose logs aigateway | head -50
```

### Database connection string errors

Ensure:
- `./data` directory exists on host: `mkdir -p data`
- SQLite database path is `/data/apikeys.sqlite`
- Container user can write to `/data` (see UID/GID section above)

### Docker Compose build fails

```bash
# Clean rebuild
docker compose down
docker system prune -f
docker compose up -d --build
```

## Project Structure

```
AiGateway/
├── Data/                      # EF Core DbContext and migrations
├── Dtos/                      # Data transfer objects
├── Endpoints/                 # Minimal API endpoint definitions
├── Middleware/                # Custom middleware
├── Services/                  # Business logic (Ollama, hashing, etc.)
├── appsettings.json          # Default configuration
├── appsettings.Development.json
├── Program.cs                 # Application startup
└── AiGateway.csproj

Docker/
├── Dockerfile                 # Multi-stage Docker build
├── docker-compose.yml         # Docker Compose configuration
├── entrypoint.sh             # Permission handling script
├── .dockerignore             # Docker ignore patterns
└── .env.example              # Example environment variables
```

## License

Proprietary - All rights reserved
