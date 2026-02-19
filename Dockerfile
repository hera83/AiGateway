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

# Install su-exec for dropping privileges and curl for healthchecks
RUN apt-get update && apt-get install -y --no-install-recommends \
    su-exec \
    curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Create non-root user with configurable UID/GID
RUN groupadd -r -g "$APP_GID" "$APP_GROUP" && \
    useradd -r -u "$APP_UID" -g "$APP_GROUP" "$APP_USER"

# Create directories for data and logs (will be overridden by volume mounts)
RUN mkdir -p /data /logs && \
    chown -R "$APP_UID:$APP_GID" /data /logs /app && \
    chmod -R 755 /data /logs

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

# Health check using curl (installed above)
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Use entrypoint script to handle permissions and start app
ENTRYPOINT ["/app/entrypoint.sh"]
