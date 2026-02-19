# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["AiGateway/AiGateway.csproj", "AiGateway/"]
RUN dotnet restore "AiGateway/AiGateway.csproj"

# Copy everything else and build
COPY AiGateway/ AiGateway/
WORKDIR /src/AiGateway
RUN dotnet build "AiGateway.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Build arguments for user/group configuration
ARG APP_UID=1000
ARG APP_GID=1000
ARG APP_USER=appuser
ARG APP_GROUP=appuser

# Install gosu for dropping privileges and curl for healthchecks
RUN apt-get update && apt-get install -y --no-install-recommends \
    gosu \
    curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Create non-root user/group idempotently
# Check if group exists before creating, reuse if found
RUN if ! getent group "$APP_GID" > /dev/null 2>&1; then \
        groupadd -r -g "$APP_GID" "$APP_GROUP"; \
    else \
        echo "Group GID $APP_GID already exists"; \
    fi

# Check if user exists before creating, reuse if found
RUN if ! getent passwd "$APP_UID" > /dev/null 2>&1; then \
        useradd -r -u "$APP_UID" -g "$APP_GID" "$APP_USER"; \
    else \
        echo "User UID $APP_UID already exists"; \
    fi

# Create directories for data and logs (will be overridden by volume mounts)
RUN mkdir -p /data /logs && \
    chmod 755 /data /logs

# Copy published app
COPY --from=publish /app/publish .

# Copy entrypoint script
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_ENVIRONMENT=Production \
    APP_UID=$APP_UID \
    APP_GID=$APP_GID \
    APP_USER=$APP_USER \
    APP_GROUP=$APP_GROUP

EXPOSE 8080

# Health check using curl
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run as root so entrypoint script can fix permissions before dropping to appuser
USER root

# Use entrypoint script to handle permissions and start app
ENTRYPOINT ["/app/entrypoint.sh"]
