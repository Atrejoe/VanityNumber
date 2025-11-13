# Kubernetes Deployment Guide

This guide covers deploying the Vanity Number application to Kubernetes with multiple configuration options.

## Architecture Options

### Option 1: Gateway (Recommended)
Uses YARP reverse proxy to serve both API and frontend under a single service:
- **Gateway**: Serves everything through one LoadBalancer
- **API**: ClusterIP (internal only)
- **Web**: ClusterIP (internal only)
- **Benefits**: Single domain, simplified CORS, one SSL certificate, path-based routing

### Option 2: Direct Access
Exposes API and Web separately:
- **API**: ClusterIP (internal only)
- **Web**: LoadBalancer (public)
- **Benefits**: Simpler architecture, fewer components

## Configuration

### ConfigMap (`kubernetes-configmap.yaml`)

The ConfigMap contains all configurable values:

```yaml
# CORS Origins (for API)
CORS_ORIGINS: "http://vanity-robertsirre-nl-web,https://vanity.robertsirre.nl"

# API Base URL (for frontend)
# With Gateway: "/api/" (same origin, no CORS needed)
# Without Gateway: "http://vanity-robertsirre-nl-api:8080/" (cross-origin)
API_BASE_URL: "/api/"
```

**Important**: Update these values for your environment:

- Replace `vanity.robertsirre.nl` with your actual domain
- For Gateway setup, use `API_BASE_URL: "/api/"`
- For Direct setup, use `API_BASE_URL: "http://vanity-robertsirre-nl-api:8080/"`

## Deployment Steps

### Prerequisites

1. Docker images built and pushed to registry:

   ```bash
   # Build images
   docker build -t atreyu/vanitynumber.api:0.1.0 -f VanityNumber.Api/Dockerfile .
   docker build -t atreyu/vanitynumber.web:0.1.0 -f VanityNumber.Web/Dockerfile .
   docker build -t atreyu/vanitynumber.gateway:0.1.0 -f gateway/Dockerfile .
   
   # Push to registry
   docker push atreyu/vanitynumber.api:0.1.0
   docker push atreyu/vanitynumber.web:0.1.0
   docker push atreyu/vanitynumber.gateway:0.1.0
   ```

**Note**: The gateway image contains nginx with a startup script that configures backend URLs from environment variables.

### Option 1: Deploy with Gateway

1. **Update ConfigMap** for gateway mode:

   ```yaml
   API_BASE_URL: "/api/"
   CORS_ORIGINS: "https://vanity.robertsirre.nl,http://vanity.robertsirre.nl"
   ```

2. **Apply resources**:

   ```bash
   kubectl apply -f Kubernetes/kubernetes-namespace.yaml
   kubectl apply -f Kubernetes/kubernetes-configmap.yaml
   kubectl apply -f Kubernetes/kubernetes-deployment-api.yaml
   kubectl apply -f Kubernetes/kubernetes-service-api.yaml
   kubectl apply -f Kubernetes/kubernetes-deployment-web.yaml
   kubectl apply -f Kubernetes/kubernetes-service-web.yaml
   kubectl apply -f Kubernetes/kubernetes-deployment-gateway.yaml
   kubectl apply -f Kubernetes/kubernetes-service-gateway.yaml
   ```

   **Note**: Gateway backend URLs are configured via environment variables in the deployment, not ConfigMap.

3. **Get LoadBalancer IP**:

   ```bash
   kubectl get svc -n vanity-robertsirre-nl vanity-robertsirre-nl-gateway
   ```

4. **Access the application**:
   - Frontend: `http://<GATEWAY-IP>/`
   - API: `http://<GATEWAY-IP>/api/`
   - Swagger: `http://<GATEWAY-IP>/swagger/`

### Option 2: Deploy without Gateway

1. **Update ConfigMap** for direct mode:

   ```yaml
   API_BASE_URL: "http://vanity-robertsirre-nl-api:8080/"
   ```

2. **Update Web Service** to LoadBalancer:

   ```yaml
   # In kubernetes-service-web.yaml, change:
   type: LoadBalancer  # Instead of ClusterIP
   ```

3. **Apply resources** (skip gateway):

   ```bash
   kubectl apply -f Kubernetes/kubernetes-namespace.yaml
   kubectl apply -f Kubernetes/kubernetes-configmap.yaml
   kubectl apply -f Kubernetes/kubernetes-deployment-api.yaml
   kubectl apply -f Kubernetes/kubernetes-service-api.yaml
   kubectl apply -f Kubernetes/kubernetes-deployment-web.yaml
   kubectl apply -f Kubernetes/kubernetes-service-web.yaml
   ```

4. **Get LoadBalancer IP**:

   ```bash
   kubectl get svc -n vanity-robertsirre-nl vanity-robertsirre-nl-web
   ```

5. **Access the application**:
   - Frontend: `http://<WEB-IP>/`

## Configuration Details

### API Configuration (Environment Variables)
- `ASPNETCORE_ENVIRONMENT`: `Production` or `Development`
- `ASPNETCORE_URLS`: `http://0.0.0.0:8080` (required for container)
- `CORS_ORIGINS`: Comma-separated list of allowed origins

### Web Configuration (Environment Variables)
- `API_BASE_URL`: Base URL for API calls
  - Gateway mode: `/api/` (relative path)
  - Direct mode: Full URL with service name

### Gateway Configuration (Environment Variables)
- `API_BACKEND_URL`: Backend API service URL (default: `vanity-robertsirre-nl-api:8080`)
- `WEB_BACKEND_URL`: Backend Web service URL (default: `vanity-robertsirre-nl-web:8080`)
- Configured in deployment spec, can be changed without rebuilding image

**To change gateway backends:**
```bash
kubectl set env deployment/vanity-robertsirre-nl-gateway \
  API_BACKEND_URL=new-api:8080 \
  WEB_BACKEND_URL=new-web:8080 \
  -n vanity-robertsirre-nl
```

## Updating Configuration

To update configuration after deployment:

```bash
# Edit the ConfigMap
kubectl edit configmap vanity-config -n vanity-robertsirre-nl

# Restart deployments to pick up changes
kubectl rollout restart deployment/vanity-robertsirre-nl-api -n vanity-robertsirre-nl
kubectl rollout restart deployment/vanity-robertsirre-nl-web -n vanity-robertsirre-nl
kubectl rollout restart deployment/vanity-robertsirre-nl-gateway -n vanity-robertsirre-nl
```

## Monitoring

### Check pod status:
```bash
kubectl get pods -n vanity-robertsirre-nl
```

### View logs:
```bash
# API logs
kubectl logs -n vanity-robertsirre-nl -l component=api --tail=100 -f

# Web logs
kubectl logs -n vanity-robertsirre-nl -l component=web --tail=100 -f

# Gateway logs
kubectl logs -n vanity-robertsirre-nl -l component=gateway --tail=100 -f
```

### Check services:
```bash
kubectl get svc -n vanity-robertsirre-nl
```

## Troubleshooting

### Frontend can't reach API
1. Check `API_BASE_URL` in ConfigMap
2. Verify CORS origins include frontend URL
3. Check pod logs for errors

### Gateway routing issues
1. Verify service names in gateway environment variables
2. Check YARP configuration in gateway logs
3. Ensure API and Web services are running

### Pod failures
1. Check pod events: `kubectl describe pod <pod-name> -n vanity-robertsirre-nl`
2. Check security context settings match image (user ID)
3. Verify resource limits are sufficient

## Resource Files

- `kubernetes-namespace.yaml`: Namespace definition
- `kubernetes-configmap.yaml`: Application configuration values
- `kubernetes-deployment-api.yaml`: API deployment (4 replicas)
- `kubernetes-deployment-web.yaml`: Web deployment (2 replicas)
- `kubernetes-deployment-gateway.yaml`: Gateway deployment (2 replicas, custom nginx image)
- `kubernetes-service-api.yaml`: API service (ClusterIP)
- `kubernetes-service-web.yaml`: Web service (ClusterIP or LoadBalancer)
- `kubernetes-service-gateway.yaml`: Gateway service (LoadBalancer)

**Note**: `kubernetes-configmap-gateway.yaml` is deprecated - gateway now uses environment variables instead.
