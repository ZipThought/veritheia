#!/bin/bash
# Database Migration Script for Veritheia
# This script applies EF Core migrations to the PostgreSQL database

set -e  # Exit on any error

echo "=== Veritheia Database Migration ==="

# Check if we're in the right directory
if [ ! -f "veritheia.Data/veritheia.Data.csproj" ]; then
    echo "Error: Must be run from the project root directory"
    exit 1
fi

# Function to get PostgreSQL container info
get_postgres_info() {
    local container_name=$1
    if docker ps --format "table {{.Names}}\t{{.Ports}}" | grep -q "$container_name"; then
        local port=$(docker ps --format "table {{.Names}}\t{{.Ports}}" | grep "$container_name" | grep -o '127.0.0.1:[0-9]*' | cut -d: -f2)
        local password=$(docker exec "$container_name" env | grep POSTGRES_PASSWORD | cut -d= -f2)
        echo "$port:$password"
    fi
}

# Try to find the Aspire PostgreSQL container
echo "Looking for Aspire PostgreSQL container..."
aspire_container=$(docker ps --format "{{.Names}}" | grep -E "postgre-|postgres-" | head -1)

if [ -z "$aspire_container" ]; then
    echo "Error: No Aspire PostgreSQL container found"
    echo "Available PostgreSQL containers:"
    docker ps | grep postgres || echo "No PostgreSQL containers running"
    exit 1
fi

echo "Found Aspire container: $aspire_container"

# Get connection details
postgres_info=$(get_postgres_info "$aspire_container")
if [ -z "$postgres_info" ]; then
    echo "Error: Could not get PostgreSQL connection info"
    exit 1
fi

port=$(echo "$postgres_info" | cut -d: -f1)
password=$(echo "$postgres_info" | cut -d: -f2)

echo "Connecting to PostgreSQL on port: $port"

# Build connection string
connection_string="Host=127.0.0.1;Port=$port;Database=veritheiadb;Username=postgres;Password=$password"

# Apply migrations
echo "Applying EF Core migrations..."
cd veritheia.Data
dotnet ef database update --connection "$connection_string"

if [ $? -eq 0 ]; then
    echo "✅ Database migrations applied successfully!"
else
    echo "❌ Migration failed!"
    exit 1
fi

echo "=== Migration Complete ==="
