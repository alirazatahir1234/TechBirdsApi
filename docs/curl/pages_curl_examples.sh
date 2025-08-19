#!/usr/bin/env bash
# Pages API cURL examples (usable in shell or importable into Postman via "Import -> Raw text")
#
# Usage:
#   1) Optionally export these variables (or edit inline below):
#        export BASE_URL=http://localhost:5000
#        export TOKEN=REPLACE_WITH_JWT
#        export PAGE_ID=00000000-0000-0000-0000-000000000000
#        export REVISION_ID=00000000-0000-0000-0000-000000000000
#        export SLUG=about-us
#   2) Run any command below in your terminal OR copy a single curl command into Postman "Import" -> "Raw text".
#
# Notes:
#   - Endpoints marked [AUTH] require a Bearer token and the appropriate role as described in the controller attributes.
#   - Controller route base: /api/Pages
#   - Roles required:
#       Create: Author,Editor,Admin,SuperAdmin
#       Update: Author,Editor,Admin,SuperAdmin (author can only update own pages; editors/admins can update any)
#       Delete (soft): Editor,Admin,SuperAdmin
#       Hard delete: Admin,SuperAdmin
#       Revisions, Restore: Author,Editor,Admin,SuperAdmin

set -euo pipefail

: "${BASE_URL:=http://localhost:5000}"
: "${TOKEN:=REPLACE_WITH_JWT}"
: "${PAGE_ID:=00000000-0000-0000-0000-000000000000}"
: "${REVISION_ID:=00000000-0000-0000-0000-000000000000}"
: "${SLUG:=about-us}"

# ---------------------------------------------------------
# [AUTH] Create a page (POST /api/Pages)
# ---------------------------------------------------------
# Minimal body: { "title": "..." }
# Status: draft|published|private
# If "slug" is omitted, it will be generated from the title and made unique.
# "published" status sets PublishedAt automatically.

cat <<'CMD'
curl -i -sS -X POST "$BASE_URL/api/Pages" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "About Us",
    "content": "<p>About TechBirds.</p>",
    "excerpt": "About page excerpt",
    "status": "draft",
    "parentId": null,
    "menuOrder": 0,
    "template": null,
    "featuredMediaId": null,
    "seoTitle": "About Us - TechBirds",
    "seoDescription": "Learn more about TechBirds",
    "metaJson": "{\"key\":\"value\"}",
    "slug": "about-us",
    "changeSummary": "Initial version"
  }'
CMD

# ---------------------------------------------------------
# [AUTH] Update a page (PUT /api/Pages/{id})
# ---------------------------------------------------------

cat <<'CMD'
curl -i -sS -X PUT "$BASE_URL/api/Pages/$PAGE_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "About TechBirds",
    "content": "<p>Updated About content.</p>",
    "excerpt": "Updated excerpt",
    "status": "published",
    "menuOrder": 1,
    "seoTitle": "About TechBirds - Updated",
    "seoDescription": "Updated SEO description",
    "changeSummary": "Publish updated content"
  }'
CMD

# ---------------------------------------------------------
# Get page by id (GET /api/Pages/{id})
# ---------------------------------------------------------

cat <<'CMD'
curl -i -sS "$BASE_URL/api/Pages/$PAGE_ID"
CMD

# ---------------------------------------------------------
# Get page by slug (GET /api/Pages/slug/{slug})
# ---------------------------------------------------------
# Public endpoint returns only published pages for anonymous users.

cat <<'CMD'
curl -i -sS "$BASE_URL/api/Pages/slug/$SLUG"
CMD

# ---------------------------------------------------------
# List pages (GET /api/Pages)
# ---------------------------------------------------------
# Query params:
#   page, limit, search, status, parentId, sortBy (created|updated|title|menu), sortOrder (asc|desc)

cat <<'CMD'
curl -i -sS "$BASE_URL/api/Pages?page=1&limit=20&search=about&status=published&sortBy=created&sortOrder=desc"
CMD

# ---------------------------------------------------------
# [AUTH] Soft delete a page (DELETE /api/Pages/{id})
# ---------------------------------------------------------

cat <<'CMD'
curl -i -sS -X DELETE "$BASE_URL/api/Pages/$PAGE_ID" \
  -H "Authorization: Bearer $TOKEN"
CMD

# ---------------------------------------------------------
# [AUTH] Hard delete a page (DELETE /api/Pages/{id}/hard)
# ---------------------------------------------------------

cat <<'CMD'
curl -i -sS -X DELETE "$BASE_URL/api/Pages/$PAGE_ID/hard" \
  -H "Authorization: Bearer $TOKEN"
CMD

# ---------------------------------------------------------
# [AUTH] List revisions (GET /api/Pages/{id}/revisions)
# ---------------------------------------------------------

cat <<'CMD'
curl -i -sS "$BASE_URL/api/Pages/$PAGE_ID/revisions" \
  -H "Authorization: Bearer $TOKEN"
CMD

# ---------------------------------------------------------
# [AUTH] Restore a revision (POST /api/Pages/{id}/restore/{revisionId})
# ---------------------------------------------------------

cat <<'CMD'
curl -i -sS -X POST "$BASE_URL/api/Pages/$PAGE_ID/restore/$REVISION_ID" \
  -H "Authorization: Bearer $TOKEN"
CMD

# End of Pages cURL examples
