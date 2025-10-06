# TechBirds Docker Setup

This Docker Compose configuration sets up the complete TechBirds application stack including:

- **PostgreSQL Database** (Port 5433)
- **pgAdmin** (Port 8080)
- **TechBirds .NET API** (Port 5001)

## Prerequisites

1. **Docker** and **Docker Compose** installed
2. **.env** file with proper database credentials

## Directory Structure
```
TechBirdsApi/
├── docker-compose.yml
├── Dockerfile
├── .dockerignore
├── .env
└── (rest of your .NET API files)
```

## Setup Instructions

### 1. Environment Configuration
Update the `.env` file with your preferred settings:
- Database credentials
- API URLs
- Port configurations

### 2. Build and Run

#### Start all services:
```bash
docker-compose up -d
```

#### Build and start (force rebuild):
```bash
docker-compose up --build -d
```

#### Stop all services:
```bash
docker-compose down
```

#### Stop and remove volumes (WARNING: This deletes data):
```bash
docker-compose down -v
```

## Service URLs

| Service | URL | Credentials |
|---------|-----|-------------|
| API | http://localhost:5001 | - |
| API Swagger | http://localhost:5001/swagger | - |
| pgAdmin | http://localhost:8080 | admin / qwerty |
| PostgreSQL | localhost:5433 | admin / qwerty |

## Database Connection in pgAdmin

1. Open http://localhost:8080
2. Login with: `admin` / `qwerty`
3. Add New Server:
   - **Name**: TechBirds
   - **Host**: `postgres` (service name)
   - **Port**: `5432`
   - **Database**: `techbirds_db`
   - **Username**: `admin`
   - **Password**: `qwerty`

## Development Workflow

### Backend Development
```bash
# Rebuild only the API
docker-compose build techbirds-backend
docker-compose up -d techbirds-backend

# View API logs
docker-compose logs -f techbirds-backend
```

### Database Management
```bash
# Access PostgreSQL directly
docker-compose exec postgres psql -U admin -d techbirds_db

# Backup database
docker-compose exec postgres pg_dump -U admin techbirds_db > backup.sql

# Restore database
docker-compose exec -T postgres psql -U admin -d techbirds_db < backup.sql
```

## Troubleshooting

### Port Conflicts
If ports are already in use, modify the `.env` file or docker-compose.yml:
```yaml
ports:
  - "5002:5001"  # Use port 5002 instead of 5001
```

### Database Connection Issues
1. Ensure PostgreSQL is fully started before the API
2. Check connection string in the API configuration
3. Verify network connectivity between containers

### Build Issues
```bash
# Clean build (remove old images)
docker-compose build --no-cache

# Remove all containers and rebuild
docker-compose down
docker system prune -f
docker-compose up --build -d
```

## Production Considerations

1. **Security**: Change default passwords in production
2. **SSL**: Configure HTTPS for production
3. **Volumes**: Use named volumes for data persistence
4. **Environment**: Set `ASPNETCORE_ENVIRONMENT=Production`
5. **Resources**: Configure resource limits for containers