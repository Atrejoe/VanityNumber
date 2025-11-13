# Configuration and Deployment Updates

This document summarizes the changes made to enable configurable API and CORS settings, plus Kubernetes deployment with optional reverse proxy.

## Changes Made

### 1. API Configuration (CORS)

**File**: `VanityNumber.Api/appsettings.json`
- Added `CorsOrigins` array for configurable CORS URLs
- Default values include localhost ports for development

**File**: `VanityNumber.Api/Program.cs`
- Updated to read CORS origins from configuration
- Supports both `CORS_ORIGINS` environment variable (comma-separated) and `CorsOrigins` from appsettings.json
- Environment variable takes precedence for Kubernetes deployments

### 2. Web Configuration (API Base URL)

**File**: `VanityNumber.Web/Dockerfile`
- Added runtime configuration script support
- Copies `update-config.sh` to update appsettings.json at container startup

**File**: `VanityNumber.Web/update-config.sh` (NEW)
- Shell script that updates `appsettings.json` with `API_BASE_URL` environment variable
- Allows runtime configuration without rebuilding the image

### 3. Kubernetes Resources

#### Configuration
**File**: `Kubernetes/kubernetes-configmap.yaml` (NEW)
- Centralized configuration for all services
- `CORS_ORIGINS`: API CORS configuration
- `API_BASE_URL`: Frontend API endpoint configuration

#### API Resources
**Files**: 
- `Kubernetes/kubernetes-deployment-api.yaml` (NEW)
- `Kubernetes/kubernetes-service-api.yaml` (NEW)

Features:
- 4 replicas with pod anti-affinity
- ClusterIP service (internal only)
- ConfigMap integration for environment variables
- Health checks via swagger endpoint
- Non-root security context

#### Web Resources
**Files**:
- `Kubernetes/kubernetes-deployment-web.yaml` (NEW)
- `Kubernetes/kubernetes-service-web.yaml` (NEW)

Features:
- 2 replicas with pod anti-affinity
- ClusterIP service (can be changed to LoadBalancer for direct access)
- API_BASE_URL from ConfigMap
- Health checks via /health endpoint
- Non-root nginx user

#### Gateway Resources (Nginx Reverse Proxy)
**Files**:
- `gateway/nginx.conf` (Docker Compose configuration)
- `gateway/nginx-k8s.conf` (Kubernetes service names)
- `gateway/Dockerfile` (Simple nginx:alpine with custom config)
- `Kubernetes/kubernetes-configmap-gateway.yaml` (NEW)
- `Kubernetes/kubernetes-deployment-gateway.yaml` (NEW)
- `Kubernetes/kubernetes-service-gateway.yaml` (NEW)

Features:
- Lightweight nginx reverse proxy for unified entry point
- Routes `/api/*` to API service
- Routes `/swagger/*` to API service
- Routes everything else to Web service
- 2 replicas with pod anti-affinity
- LoadBalancer service for public access
- Only ~5MB image size (nginx:alpine)
- No custom code, pure configuration

### 4. Docker Compose Updates

**File**: `docker-compose.yml`
- Added environment variables for API CORS configuration
- Added environment variable for Web API base URL
- Added `vanity-gateway` service with nginx reverse proxy
- All services properly linked with dependencies

### 5. Documentation

**File**: `Kubernetes/README.md` (NEW)
- Comprehensive deployment guide
- Two deployment options (with/without gateway)
- Configuration instructions
- Troubleshooting guide
- Monitoring commands

## Configuration Options

### Local Development (Docker Compose)
```bash
# Access services directly:
# - API: http://localhost:8081
# - Web: http://localhost:8082
# - Gateway: http://localhost:8083
docker compose up -d
```

### Kubernetes with Gateway (Recommended)
```bash
# Single entry point for everything:
# - Frontend: http://<gateway-ip>/
# - API: http://<gateway-ip>/api/
# - Swagger: http://<gateway-ip>/swagger/

# Update ConfigMap: API_BASE_URL: "/api/"
kubectl apply -f Kubernetes/
```

### Kubernetes without Gateway
```bash
# Direct access to frontend:
# - Frontend: http://<web-ip>/

# Update kubernetes-service-web.yaml: type: LoadBalancer
# Update ConfigMap: API_BASE_URL: "http://vanity-robertsirre-nl-api:8080/"
kubectl apply -f Kubernetes/kubernetes-namespace.yaml
kubectl apply -f Kubernetes/kubernetes-configmap.yaml
kubectl apply -f Kubernetes/kubernetes-deployment-api.yaml
kubectl apply -f Kubernetes/kubernetes-service-api.yaml
kubectl apply -f Kubernetes/kubernetes-deployment-web.yaml
kubectl apply -f Kubernetes/kubernetes-service-web.yaml
```

## Environment Variables Reference

### API
- `CORS_ORIGINS`: Comma-separated allowed origins (e.g., "http://localhost:8082,https://myapp.com")
- `ASPNETCORE_ENVIRONMENT`: Environment name (Production, Development)
- `ASPNETCORE_URLS`: Listening URLs (e.g., "http://0.0.0.0:8080")

### Web
- `API_BASE_URL`: API endpoint URL
  - Gateway mode: "/api/" (relative, same origin)
  - Direct mode: "http://api-service:8080/" (full URL)

### Gateway
- Configuration managed via nginx.conf (Docker) or ConfigMap (Kubernetes)
- Nginx configuration file defines all routing
- No environment variables needed - all routing is in nginx.conf
- Upstream servers point to backend services

## Benefits of Gateway Approach

1. **Single Domain**: API and frontend served from same domain
2. **Simplified CORS**: No cross-origin requests when using relative URLs
3. **SSL/TLS**: Only need one certificate for the gateway
4. **Path-based Routing**: Clean URL structure (/api/, /swagger/, /)
5. **Flexibility**: Easy to add more services or routing rules
6. **Security**: Internal services not exposed directly

## Next Steps

1. Build and push Docker images to registry
2. Update ConfigMap with your domain names
3. Choose deployment option (with or without gateway)
4. Deploy to Kubernetes cluster
5. Configure DNS/Ingress for production domain
6. Add SSL/TLS certificates (via Ingress or LoadBalancer annotations)
