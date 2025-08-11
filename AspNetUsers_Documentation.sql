-- ============================================================================
-- 🗃️ TechBirds AspNetUsers Table Schema Documentation
-- ============================================================================
-- Complete schema for AspNetUsers table with custom fields

-- 🔍 QUICK SCHEMA LOOKUP COMMANDS:

-- 1. View table structure
-- \d+ "AspNetUsers"

-- 2. Get column details
-- SELECT column_name, data_type, is_nullable, character_maximum_length 
-- FROM information_schema.columns WHERE table_name = 'AspNetUsers';

-- 📊 ASPNETUSERS TABLE SCHEMA (Based on ApplicationUser + IdentityUser)
/*
===========================================
            ASPNETUSERS COLUMNS
===========================================

IDENTITY FRAMEWORK COLUMNS (Standard):
---------------------------------------
Id                          - text (GUID Primary Key)
UserName                    - character varying(256)
NormalizedUserName          - character varying(256)
Email                       - character varying(256)
NormalizedEmail             - character varying(256)
EmailConfirmed              - boolean
PasswordHash                - text
SecurityStamp               - text
ConcurrencyStamp            - text
PhoneNumber                 - text
PhoneNumberConfirmed        - boolean
TwoFactorEnabled           - boolean
LockoutEnd                 - timestamp with time zone
LockoutEnabled             - boolean
AccessFailedCount          - integer

CUSTOM APPLICATION FIELDS:
--------------------------
Name                       - text (NOT NULL)
FirstName                  - text (NOT NULL)
LastName                   - text (NOT NULL)
Bio                        - text (NOT NULL, DEFAULT '')

PROFILE & MEDIA FIELDS:
----------------------
Avatar                     - text (NULL)
Website                    - text (NULL)
Twitter                    - text (NULL)
LinkedIn                   - text (NULL)
Specialization            - text (NULL)

CONTENT CREATOR STATS:
----------------------
ArticleCount              - integer (NOT NULL, DEFAULT 0)
PostsCount                - integer (NOT NULL, DEFAULT 0)
TotalViews                - integer (NOT NULL, DEFAULT 0)
LastActive                - timestamp with time zone (NULL)
JoinedAt                  - timestamp with time zone (NOT NULL, DEFAULT NOW())

TIMESTAMPS:
-----------
CreatedAt                 - timestamp with time zone (NOT NULL, DEFAULT NOW())
UpdatedAt                 - timestamp with time zone (NULL)

===========================================
*/

-- 🎯 QUICK QUERIES TO EXPLORE THE TABLE:

-- View all users with basic info
SELECT 
    "Id",
    "FirstName",
    "LastName", 
    "Email",
    "CreatedAt"
FROM "AspNetUsers" 
ORDER BY "CreatedAt" DESC;

-- Count users by creation date
SELECT 
    DATE("CreatedAt") as created_date,
    COUNT(*) as user_count
FROM "AspNetUsers"
GROUP BY DATE("CreatedAt")
ORDER BY created_date DESC;

-- Users with profile information
SELECT 
    "FirstName" || ' ' || "LastName" as full_name,
    "Email",
    "Bio",
    "Website",
    "Specialization",
    "ArticleCount",
    "PostsCount"
FROM "AspNetUsers"
WHERE "Bio" IS NOT NULL AND "Bio" != '';

-- Users by role
SELECT 
    r."Name" as role_name,
    u."FirstName",
    u."LastName",
    u."Email"
FROM "AspNetUsers" u
JOIN "AspNetUserRoles" ur ON u."Id" = ur."UserId"
JOIN "AspNetRoles" r ON ur."RoleId" = r."Id"
ORDER BY r."Name", u."FirstName";

-- 📈 STATISTICS QUERIES:

-- Total users by role
SELECT 
    r."Name" as role,
    COUNT(*) as count
FROM "AspNetRoles" r
LEFT JOIN "AspNetUserRoles" ur ON r."Id" = ur."RoleId"
GROUP BY r."Name"
ORDER BY count DESC;

-- Content statistics
SELECT 
    AVG("ArticleCount") as avg_articles,
    AVG("PostsCount") as avg_posts,
    AVG("TotalViews") as avg_views,
    MAX("ArticleCount") as max_articles,
    MAX("PostsCount") as max_posts
FROM "AspNetUsers";

-- 🔧 USEFUL MAINTENANCE QUERIES:

-- Find inactive users (no LastActive)
SELECT 
    "FirstName",
    "LastName",
    "Email",
    "CreatedAt"
FROM "AspNetUsers"
WHERE "LastActive" IS NULL
ORDER BY "CreatedAt" DESC;

-- Users without complete profiles
SELECT 
    "FirstName",
    "LastName", 
    "Email",
    CASE WHEN "Bio" = '' OR "Bio" IS NULL THEN 'Missing Bio' ELSE 'Has Bio' END as bio_status,
    CASE WHEN "Avatar" IS NULL THEN 'No Avatar' ELSE 'Has Avatar' END as avatar_status
FROM "AspNetUsers"
ORDER BY "CreatedAt" DESC;
