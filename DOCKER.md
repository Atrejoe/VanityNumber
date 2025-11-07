# Vanity Number API - Docker Instructions

## Quick Start

### Using Docker Compose (Recommended)

```bash
# Build and run
docker-compose up -d

# View logs
docker-compose logs -f

# Stop
docker-compose down
```

The API will be available at: http://localhost:5000

Swagger UI will be at: http://localhost:5000

### Using Docker CLI

```bash
# Build the image
docker build -t vanity-number-api .

# Run the container
docker run -d -p 5000:8080 --name vanity-api vanity-number-api

# View logs
docker logs -f vanity-api

# Stop and remove
docker stop vanity-api
docker rm vanity-api
```

## API Endpoints

- **Swagger UI**: http://localhost:5000
- **OpenAPI Spec**: http://localhost:5000/swagger/v1/swagger.json
- **Health Check**: http://localhost:5000/api/vanitynumber/convert?phoneNumber=1234567890&dictionaries=Dutch

## Configuration

The Dockerfile uses .NET 10 RC and sets:
- Port: 8080 (mapped to 5000 on host)
- Environment: Production
- Health checks: Enabled via ASP.NET Core

## Build Arguments

To customize the build:

```bash
docker build \
  --build-arg ASPNETCORE_ENVIRONMENT=Development \
  -t vanity-number-api .
```

## Troubleshooting

### Port Already in Use
If port 5000 is already in use, change it in `docker-compose.yml`:
```yaml
ports:
  - "8080:8080"  # Use port 8080 instead
```

### View Container Logs
```bash
docker-compose logs -f vanity-api
```

### Rebuild After Code Changes
```bash
docker-compose up -d --build
```

## Features

- Leet speak support (0=O, 1=I/L, 4=A, 5=S, 7=T, 8=B)
- Partial matching (matches anywhere in phone number)
- Multiple dictionaries (Dutch, English, Urban)
- Prioritizes longer matches
- Preserves diacritics in results
- 95.5% code coverage
