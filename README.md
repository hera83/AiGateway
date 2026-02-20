# AiGateway

En produktionsklar .NET 10 API Gateway til AI-tjenester med transparent request forwarding, autentificering og API-nøgle management.

## Hvad er AiGateway?

AiGateway fungerer som en sikker proxy mellem dine applikationer og AI-tjenester (Ollama og Speaches):

- **Autentificering**: x-api-key baseret adgangskontrol med master keys (administration) og client keys (AI-tjenester)
- **Proxy**: Transparent forwarding af requests til upstream services uden at afsløre gateway credentials
- **Streaming**: Fuld support for streaming responses fra Ollama
- **Logging**: Struktureret logging med Serilog

## Installation

### Docker (Anbefalet)

**Forudsætninger:**
- Docker & Docker Compose installeret
- Adgang til Ollama og Speaches services

**Hurtig start:**

1. **Klon repository:**
   ```bash
   git clone https://github.com/hera83/AiGateway.git
   cd AiGateway
   ```

2. **Opret required directories:**
   ```bash
   mkdir -p data logs
   ```

3. **Konfigurer environment:**
   ```bash
   cp .env.example .env
   ```
   
   Rediger `.env` og sæt:
   ```env
   AIGATEWAY_MASTER_KEY=dit-unikke-guid-her
   OLLAMA_BASE_URL=http://10.64.10.5:11434
   SPEACHES_BASE_URL=http://10.64.10.4:8000
   
   # For store lydfiler (default 300MB, øg hvis nødvendigt)
   Gateway__MaxRequestBodyMb=300
   ```

4. **Start gateway:**
   ```bash
   docker compose up -d --build
   ```

5. **Verificer at det kører:**
   ```bash
   docker compose logs -f aigateway
   ```

**Adgang:**
- **API**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger
- **Health check**: http://localhost:8080/health

**Bemærk store audio uploads:**
Hvis du får fejlen "Request body too large" ved upload af store lydfiler til Speaches transcription endpoint, øg `Gateway__MaxRequestBodyMb` i `.env` filen:
```env
# For filer op til 500MB
Gateway__MaxRequestBodyMb=500
```

### Lokal udvikling (uden Docker)

**Forudsætninger:**
- .NET 10 SDK

**Start:**

1. **Naviger til projekt:**
   ```bash
   cd AiGateway
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Konfigurer appsettings.json** med dine upstream URLs og master key

4. **Kør applikationen:**
   ```bash
   dotnet run
   ```

Applikationen starter på https://localhost:5001 og http://localhost:5000.

## Basis brug

### 1. Opret en client key (kræver master key)

```bash
curl -X POST http://localhost:8080/v1/keys/create \
  -H "x-api-key: DIT-MASTER-KEY" \
  -H "Content-Type: application/json" \
  -d '{"appName":"MinApp","appContact":"kontakt@example.com"}'
```

### 2. Brug client key til at kalde Ollama

```bash
curl -X POST http://localhost:8080/v1/ollama/api/generate \
  -H "x-api-key: DIN-CLIENT-KEY" \
  -H "Content-Type: application/json" \
  -d '{"model":"gemma3:12b","prompt":"Hej verden","stream":false}'
```

## Adgangskontrol

- **Master keys**: Kan kun administrere API-nøgler (`/v1/keys`)
- **Client keys**: Kan kun tilgå AI-tjenester (`/v1/ollama`, `/v1/speaches`)
- **Offentlig adgang**: `/health`, `/swagger` (ingen autentificering)

## Teknisk information

- **.NET 10** Minimal API
- **SQLite** database (EF Core migrations)
- **Serilog** logging (console + file)
- **Docker** support med persistent volumes
- **Swagger/OpenAPI** dokumentation

## Support

For spørgsmål eller problemer, åbn et issue på [GitHub](https://github.com/hera83/AiGateway/issues).

## Licens

Proprietary - All rights reserved
