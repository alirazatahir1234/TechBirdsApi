#!/bin/bash
# init-database.sh - Database initialization script

set -e

echo "Creating database and user if they don't exist..."

# Create database if it doesn't exist
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Create extensions if needed
    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
    
    -- Grant permissions
    GRANT ALL PRIVILEGES ON DATABASE $POSTGRES_DB TO $POSTGRES_USER;
    
    -- Create any additional setup here
    \echo 'Database initialization completed!'
EOSQL

echo "Database initialization script completed."