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
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Create directories for data and logs with correct permissions
RUN mkdir -p /data /logs && \
    chown -R appuser:appuser /data /logs /app

# Copy published app
COPY --from=publish /app/publish .

# Switch to non-root user
USER appuser

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_ENVIRONMENT=Production

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AiGateway.dll"]
