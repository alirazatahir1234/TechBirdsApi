-- ============================================================================
-- 🗃️ AspNetUsers Table Schema - TechBirds Web API
-- ============================================================================
-- This script provides complete information about the AspNetUsers table structure
-- Run this in psql to see the table schema, indexes, constraints, and data

-- 📋 1. TABLE DESCRIPTION
\echo '=== ASPNETUSERS TABLE DESCRIPTION ==='
\d+ "AspNetUsers"

-- 📊 2. TABLE SCHEMA WITH DATA TYPES
\echo ''
\echo '=== DETAILED COLUMN INFORMATION ==='
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'AspNetUsers' 
ORDER BY ordinal_position;

-- 🔑 3. PRIMARY KEY AND INDEXES
\echo ''
\echo '=== INDEXES AND CONSTRAINTS ==='
SELECT 
    indexname,
    indexdef
FROM pg_indexes 
WHERE tablename = 'AspNetUsers';

-- 🔗 4. FOREIGN KEY RELATIONSHIPS
\echo ''
\echo '=== FOREIGN KEY CONSTRAINTS ==='
SELECT
    tc.constraint_name,
    tc.table_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_name = 'AspNetUsers';

-- 📈 5. TABLE STATISTICS
\echo ''
\echo '=== TABLE STATISTICS ==='
SELECT 
    schemaname,
    tablename,
    attname as column_name,
    n_distinct,
    most_common_vals,
    most_common_freqs
FROM pg_stats 
WHERE tablename = 'AspNetUsers';

-- 👥 6. CURRENT USER COUNT BY ROLE
\echo ''
\echo '=== USER COUNT BY ROLE ==='
SELECT 
    r."Name" as role_name,
    COUNT(ur."UserId") as user_count
FROM "AspNetRoles" r
LEFT JOIN "AspNetUserRoles" ur ON r."Id" = ur."RoleId"
GROUP BY r."Name"
ORDER BY user_count DESC;

-- 👤 7. SAMPLE USER DATA (First 5 users with key fields)
\echo ''
\echo '=== SAMPLE USER DATA (First 5 Users) ==='
SELECT 
    "Id",
    "FirstName",
    "LastName",
    "Email",
    "CreatedAt",
    "JoinedAt",
    "ArticleCount",
    "PostsCount"
FROM "AspNetUsers"
ORDER BY "CreatedAt" DESC
LIMIT 5;

-- 📝 8. COMPLETE TABLE CREATE STATEMENT
\echo ''
\echo '=== COMPLETE TABLE SCHEMA ==='
SELECT 
    'CREATE TABLE "AspNetUsers" (' ||
    string_agg(
        '    "' || column_name || '" ' || 
        UPPER(data_type) ||
        CASE 
            WHEN character_maximum_length IS NOT NULL THEN '(' || character_maximum_length || ')'
            WHEN data_type = 'numeric' THEN '(' || numeric_precision || ',' || numeric_scale || ')'
            ELSE ''
        END ||
        CASE WHEN is_nullable = 'NO' THEN ' NOT NULL' ELSE '' END ||
        CASE WHEN column_default IS NOT NULL THEN ' DEFAULT ' || column_default ELSE '' END,
        ',' || E'\n'
        ORDER BY ordinal_position
    ) ||
    E'\n);' as create_statement
FROM information_schema.columns 
WHERE table_name = 'AspNetUsers';

-- 🎯 9. CUSTOM FIELDS ADDED TO IDENTITY
\echo ''
\echo '=== CUSTOM FIELDS (Beyond Standard Identity) ==='
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'AspNetUsers' 
    AND column_name NOT IN (
        'Id', 'UserName', 'NormalizedUserName', 'Email', 
        'NormalizedEmail', 'EmailConfirmed', 'PasswordHash', 
        'SecurityStamp', 'ConcurrencyStamp', 'PhoneNumber', 
        'PhoneNumberConfirmed', 'TwoFactorEnabled', 'LockoutEnd', 
        'LockoutEnabled', 'AccessFailedCount'
    )
ORDER BY ordinal_position;

-- 🔍 10. TABLE SIZE AND DISK USAGE
\echo ''
\echo '=== TABLE SIZE INFORMATION ==='
SELECT 
    pg_size_pretty(pg_total_relation_size('"AspNetUsers"')) as total_size,
    pg_size_pretty(pg_relation_size('"AspNetUsers"')) as table_size,
    (SELECT COUNT(*) FROM "AspNetUsers") as row_count;
