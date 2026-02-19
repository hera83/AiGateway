# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY AiGateway/AiGateway.csproj AiGateway/
RUN dotnet restore AiGateway/AiGateway.csproj

# Copy everything else and build
COPY AiGateway/ AiGateway/
WORKDIR /src/AiGateway
RUN dotnet build AiGateway.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish AiGateway.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create directories for data and logs
RUN mkdir -p /app/data /app/logs

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AiGateway.dll"]
