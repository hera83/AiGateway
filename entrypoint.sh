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
echo "App UID:GID: $APP_UID:$APP_GID ($APP_USER:$APP_GROUP)"

# Ensure data and logs directories exist
mkdir -p /data /logs
echo "Created directories: /data /logs"

# Check and fix permissions on volumes
echo "Checking volume permissions..."
if [ ! -w /data ]; then
    echo "WARNING: /data is not writable, fixing permissions..."
    echo "Before: $(ls -ld /data)"
    chown -R "$APP_UID:$APP_GID" /data /logs || true
    chmod -R 755 /data /logs
    echo "After:  $(ls -ld /data)"
else
    echo "✓ /data is writable"
fi

if [ ! -w /logs ]; then
    echo "WARNING: /logs is not writable, fixing permissions..."
    chown -R "$APP_UID:$APP_GID" /logs || true
    chmod -R 755 /logs
else
    echo "✓ /logs is writable"
fi

# Optional: Set umask for new files (group writable)
# umask 002

# Drop to non-root user and run application
echo "Starting application as: $APP_USER ($APP_UID:$APP_GID)"
exec gosu "$APP_UID:$APP_GID" dotnet AiGateway.dll
