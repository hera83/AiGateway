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
- **🐳 Docker Support**: Multi-stage build with persistent volumes for data and logs

## Quick Start with Docker

### Prerequisites
- Docker & Docker Compose installed
- Access to Ollama and Speaches services

### Setup

1. **Clone repository:**
   ```bash
   git clone https://github.com/hera83/AiGateway.git
   cd AiGateway
   ```

2. **Create environment file:**
   ```bash
   cp .env.example .env
   ```
   
   Edit `.env` with your configuration:
   ```env
   AIGATEWAY_MASTER_KEY=your-unique-guid-here
   SPEACHES_BASE_URL=http://10.64.10.4:8000
   OLLAMA_BASE_URL=http://10.64.10.5:11434
   ```

3. **Start the gateway:**
   ```bash
   docker compose up -d --build
   ```

4. **Verify it's running:**
   ```bash
   docker compose logs -f
   ```

### Access the Gateway

- **Swagger UI:** http://localhost:8080/swagger
- **API Base:** http://localhost:8080
- **Health Check:** http://localhost:8080/health

### Persistent Data

- **SQLite Database:** `./data/apikeys.sqlite` (mounted from `/data` in container)
- **Logs:** `./logs/` (mounted from `/logs` in container)

Both directories persist between restarts and are automatically created.

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
```

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

## Configuration

### Environment Variables (Docker)

Set these in `.env` when using Docker Compose:

| Variable | Default | Description |
|----------|---------|-------------|
| `AIGATEWAY_MASTER_KEY` | Required | GUID for internal security |
| `SPEACHES_BASE_URL` | http://10.64.10.4:8000 | Speaches service URL |
| `OLLAMA_BASE_URL` | http://10.64.10.5:11434 | Ollama service URL |
| `OLLAMA_DEFAULT_MODEL` | llama3.2 | Default Ollama model |
| `OLLAMA_TIMEOUT` | 120 | Request timeout in seconds |

### Configuration Files

- **appsettings.json** - Default configuration (overridable by env vars)
- **appsettings.Development.json** - Development-specific settings
- **.env.example** - Example environment variables for Docker

## Architecture

### Components

- **ApiKeyAuthenticationHandler**: Custom authentication handler validating x-api-key header
- **ApiKeyDbContext**: Entity Framework Core context for API key storage
- **ClientKeyService**: Business logic for key management (CRUD, validation)
- **HashingService**: SHA256 hashing with per-key random 16-byte salt
- **OllamaService**: Typed service for Ollama API calls with streaming support
- **Endpoints**: Minimal API endpoints for key management and request proxying
- **Middleware**: Error handling and global exception catching

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

### Docker container won't start
```bash
# Check logs for errors
docker compose logs aigateway

# Check if container is running
docker compose ps
```

### Database issues
```bash
# Reset database and volumes
docker compose down -v
docker compose up -d --build
```

### Connection to upstream services fails
- Verify Ollama and Speaches are running and accessible
- Check IP addresses in `.env` are correct
- Ensure network connectivity from Docker container

### HTTPS issues in container
HTTPS redirection is disabled in Docker by default (set in docker-compose.yml). 
For local development, HTTPS redirect is enabled by default.

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
└── .dockerignore             # Docker ignore patterns
```

## License

Proprietary - All rights reserved
