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

echo "Permissions before fix:"
ls -ld /data /logs || true

# If running as root, or /data is not writable, fix ownership
if [ "$(id -u)" -eq 0 ] || [ ! -w /data ]; then
    echo "Fixing ownership for /data and /logs..."
    chown -R "$APP_UID:$APP_GID" /data /logs || true
fi

# Ensure app user can read/write
chmod -R u+rwX /data /logs

echo "Permissions after fix:"
ls -ld /data /logs || true

# Start app as non-root user
exec gosu "$APP_UID:$APP_GID" dotnet AiGateway.dll
