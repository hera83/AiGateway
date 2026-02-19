#!/bin/bash
set -e

# Entrypoint script for AiGateway container
# Handles permission setup before running the application

APP_UID=${APP_UID:-1000}
APP_GID=${APP_GID:-1000}
APP_USER=${APP_USER:-appuser}
APP_GROUP=${APP_GROUP:-appuser}

echo "=== AiGateway Entrypoint ==="
echo "Running as: $(id)"
echo "App user: $APP_USER (UID: $APP_UID, GID: $APP_GID)"

# If running as root, fix permissions and drop to app user
if [ "$(id -u)" -eq 0 ]; then
    echo "Running as root - fixing permissions..."
    
    # Ensure data and logs directories exist
    mkdir -p /data /logs
    
    # Fix ownership and permissions for volumes
    # If volumes are already mounted from host, this ensures container user can access them
    chown -R "$APP_UID:$APP_GID" /data /logs
    chmod -R 755 /data /logs
    
    # Fix app directory permissions
    chown -R "$APP_UID:$APP_GID" /app
    chmod -R 755 /app
    
    echo "Permissions fixed:"
    ls -la / | grep -E "data|logs"
    
    # Drop to non-root user and run application
    echo "Dropping to non-root user: $APP_USER"
    exec su-exec "$APP_UID:$APP_GID" dotnet AiGateway.dll
else
    # Already running as non-root user
    CURRENT_UID=$(id -u)
    CURRENT_GID=$(id -g)
    echo "Already running as non-root ($CURRENT_UID:$CURRENT_GID)"
    
    # Check if /data and /logs are writable
    if [ ! -w /data ]; then
        echo "WARNING: /data is not writable by current user!"
        echo "Current permissions:"
        ls -la / | grep -E "data|logs"
        echo "You may need to adjust AIGATEWAY_UID/AIGATEWAY_GID in .env"
    fi
    
    # Run application directly
    exec dotnet AiGateway.dll
fi
