# AiGateway Docker Installation

Dette dokument beskriver hvordan du kører AiGateway i en Docker container.

## Forudsætninger

- Docker installeret på din maskine
- Docker Compose installeret (inkluderet i Docker Desktop)

## Konfiguration

Før du starter containeren, skal du tilpasse følgende environment variabler i `docker-compose.yml`:

### Upstream Services
Opdater disse URL'er til at pege på dine faktiske services:
```yaml
- Upstreams__SpeachesBaseUrl=http://10.64.10.4:8000
- Upstreams__OllamaBaseUrl=http://10.64.10.5:11434
```

### Master Key
Skift master key til en sikker værdi:
```yaml
- Gateway__MasterKey=12345678-1234-1234-1234-123456789012
```

## Byg og Start Container

### Start containeren
```bash
docker-compose up -d
```

### Stop containeren
```bash
docker-compose down
```

### Genbyg containeren efter kode ændringer
```bash
docker-compose up -d --build
```

### Se logs
```bash
docker-compose logs -f aigateway
```

## Container Information

- **Container navn**: aigateway
- **Port**: 8080 (http://localhost:8080)
- **Swagger UI**: http://localhost:8080/swagger
- **Health check**: http://localhost:8080/health (hvis implementeret)

## Data Persistens

Følgende data gemmes i Docker volumes:

- **aigateway-data**: SQLite database (API keys)
- **aigateway-logs**: Application logs

Disse volumes bevares selv når containeren genstartes eller opdateres.

### Backup af data

```bash
# Backup database
docker cp aigateway:/app/data/apikeys.sqlite ./backup/

# Backup logs
docker cp aigateway:/app/logs ./backup/
```

## Netværk

Hvis du vil køre Ollama eller andre services også i Docker, kan du tilføje dem til samme netværk:

```yaml
services:
  aigateway:
    # ... existing config
    
  ollama:
    image: ollama/ollama
    container_name: ollama
    ports:
      - "11434:11434"
    networks:
      - aigateway-network
```

Så kan du bruge `http://ollama:11434` som `Upstreams__OllamaBaseUrl`.

## Troubleshooting

### Tjek container status
```bash
docker ps
```

### Se detaljerede logs
```bash
docker logs aigateway
```

### Adgang til container shell
```bash
docker exec -it aigateway /bin/bash
```

### Ryd volumes (ADVARSEL: sletter data)
```bash
docker-compose down -v
```

## Environment Variabler

Alle konfigurationsindstillinger fra `appsettings.json` kan overstyres via environment variabler ved at bruge `__` (double underscore) som separator.

Eksempel:
- `Gateway:MasterKey` → `Gateway__MasterKey`
- `Upstreams:OllamaBaseUrl` → `Upstreams__OllamaBaseUrl`
- `Ollama:DefaultModel` → `Ollama__DefaultModel`
