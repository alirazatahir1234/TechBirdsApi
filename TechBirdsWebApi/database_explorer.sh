#!/bin/bash

# ============================================================================
# 🗃️ TechBirds Database Schema Explorer
# ============================================================================
# Run this script to connect to PostgreSQL and explore AspNetUsers table

echo "🔍 TechBirds Database Schema Explorer"
echo "====================================="

# Check if connection string is available
CONNECTION_STRING=$(grep "DefaultConnection" appsettings.json | sed 's/.*"DefaultConnection": "\(.*\)".*/\1/')

if [ -z "$CONNECTION_STRING" ]; then
    echo "❌ Could not find connection string in appsettings.json"
    exit 1
fi

echo "📡 Found database connection"
echo ""

# Extract database details (assuming PostgreSQL format)
# Format: Host=localhost;Database=techbirds_db;Username=postgres;Password=your_password

DB_HOST=$(echo $CONNECTION_STRING | grep -o 'Host=[^;]*' | cut -d'=' -f2)
DB_NAME=$(echo $CONNECTION_STRING | grep -o 'Database=[^;]*' | cut -d'=' -f2)
DB_USER=$(echo $CONNECTION_STRING | grep -o 'Username=[^;]*' | cut -d'=' -f2)

echo "🏠 Host: $DB_HOST"
echo "🗄️  Database: $DB_NAME"
echo "👤 User: $DB_USER"
echo ""

echo "🔧 PostgreSQL Commands to explore AspNetUsers:"
echo "=============================================="
echo ""
echo "1️⃣  Connect to database:"
echo "   psql -h $DB_HOST -U $DB_USER -d $DB_NAME"
echo ""
echo "2️⃣  View table structure:"
echo "   \\d+ \"AspNetUsers\""
echo ""
echo "3️⃣  Show column details:"
echo "   SELECT column_name, data_type, is_nullable, character_maximum_length"
echo "   FROM information_schema.columns"
echo "   WHERE table_name = 'AspNetUsers'"
echo "   ORDER BY ordinal_position;"
echo ""
echo "4️⃣  Count users by role:"
echo "   SELECT r.\"Name\" as role, COUNT(*) as count"
echo "   FROM \"AspNetRoles\" r"
echo "   LEFT JOIN \"AspNetUserRoles\" ur ON r.\"Id\" = ur.\"RoleId\""
echo "   GROUP BY r.\"Name\";"
echo ""
echo "5️⃣  View sample user data:"
echo "   SELECT \"Id\", \"FirstName\", \"LastName\", \"Email\", \"CreatedAt\""
echo "   FROM \"AspNetUsers\""
echo "   ORDER BY \"CreatedAt\" DESC"
echo "   LIMIT 5;"
echo ""
echo "📝 Alternative: Use the provided SQL files:"
echo "   - AspNetUsers_Schema.sql (comprehensive schema info)"
echo "   - AspNetUsers_Documentation.sql (schema docs + queries)"
echo ""
echo "💡 To run SQL file in psql:"
echo "   \\i AspNetUsers_Schema.sql"
