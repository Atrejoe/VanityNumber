# Configurable Gateway Backend URLs

The nginx gateway now supports configurable backend URLs via environment variables, allowing you to change the API and Web service addresses without rebuilding the image.

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `API_BACKEND_URL` | API service hostname and port | `vanity-api:8080` |
| `WEB_BACKEND_URL` | Web service hostname and port | `vanity-web:8080` |

## Docker Compose Usage

### Default Configuration
```yaml
vanity-gateway:
  build:
    context: .
    dockerfile: ./gateway/Dockerfile
  environment:
    - API_BACKEND_URL=vanity-api:8080
    - WEB_BACKEND_URL=vanity-web:8080
```

### Custom Backend URLs
```yaml
vanity-gateway:
  image: atreyu/vanitynumber.gateway:0.1.0
  environment:
    - API_BACKEND_URL=my-custom-api:9000
    - WEB_BACKEND_URL=my-custom-web:3000
```

### Using External Services
```yaml
vanity-gateway:
  image: atreyu/vanitynumber.gateway:0.1.0
  environment:
    - API_BACKEND_URL=api.example.com:443
    - WEB_BACKEND_URL=web.example.com:443
```

## Kubernetes Usage

### Default Configuration
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vanity-robertsirre-nl-gateway
spec:
  template:
    spec:
      containers:
      - name: gateway
        image: atreyu/vanitynumber.gateway:0.1.0
        env:
        - name: API_BACKEND_URL
          value: "vanity-robertsirre-nl-api:8080"
        - name: WEB_BACKEND_URL
          value: "vanity-robertsirre-nl-web:8080"
```

### Using ConfigMap
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: gateway-backend-config
data:
  API_BACKEND_URL: "vanity-robertsirre-nl-api:8080"
  WEB_BACKEND_URL: "vanity-robertsirre-nl-web:8080"
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vanity-robertsirre-nl-gateway
spec:
  template:
    spec:
      containers:
      - name: gateway
        image: atreyu/vanitynumber.gateway:0.1.0
        envFrom:
        - configMapRef:
            name: gateway-backend-config
```

## Changing Backend URLs

### Docker Compose
1. **Edit docker-compose.yml** to change environment variables
2. **Restart the gateway**:
   ```bash
   podman compose up -d vanity-gateway
   ```

**No rebuild needed!**

### Kubernetes
1. **Update the deployment** with new environment values:
   ```bash
   kubectl set env deployment/vanity-robertsirre-nl-gateway \
     API_BACKEND_URL=new-api:8080 \
     WEB_BACKEND_URL=new-web:8080 \
     -n vanity-robertsirre-nl
   ```

2. Or **edit the ConfigMap** and restart:
   ```bash
   kubectl edit configmap gateway-backend-config -n vanity-robertsirre-nl
   kubectl rollout restart deployment/vanity-robertsirre-nl-gateway -n vanity-robertsirre-nl
   ```

**No image rebuild needed!**

## Verification

Check the configured backend URLs:

### Docker Compose
```bash
podman exec vanity-number-gateway cat /etc/nginx/nginx.conf | grep "server"
```

### Kubernetes
```bash
kubectl exec -n vanity-robertsirre-nl deployment/vanity-robertsirre-nl-gateway -- cat /etc/nginx/nginx.conf | grep "server"
```

You should see output like:
```nginx
server vanity-api:8080;
server vanity-web:8080;
```

## How It Works

1. **Dockerfile** copies `nginx.conf` as a **template** (`nginx.conf.template`)
2. **Entrypoint script** (`docker-entrypoint.sh`) runs at container startup
3. Script uses `envsubst` to replace `${API_BACKEND_URL}` and `${WEB_BACKEND_URL}` in the template
4. Generated `nginx.conf` is written with actual values
5. Nginx starts with the configured backend URLs

## Testing Different Backends

### Test with localhost API
```bash
podman run -d --name test-gateway \
  -e API_BACKEND_URL=host.docker.internal:8081 \
  -e WEB_BACKEND_URL=host.docker.internal:8082 \
  -p 9000:8080 \
  atreyu/vanitynumber.gateway:0.1.0
```

### Test with custom services
```bash
docker-compose -f - <<EOF
services:
  custom-gateway:
    image: atreyu/vanitynumber.gateway:0.1.0
    environment:
      - API_BACKEND_URL=myapi.example.com:443
      - WEB_BACKEND_URL=myweb.example.com:443
    ports:
      - "8080:8080"
EOF
```

## Benefits

✅ **No image rebuild** when changing backend URLs  
✅ **Same image** works for dev, staging, and production  
✅ **Easy configuration** via environment variables  
✅ **Kubernetes-friendly** with ConfigMaps  
✅ **Docker Compose friendly** with env vars  

## Troubleshooting

### Gateway shows "Host is unreachable"
- Verify service names are correct for your environment
- Check services are on the same Docker/Kubernetes network
- Verify backend services are running and healthy

### Changes not applied
- Ensure you restart the gateway container after changing env vars
- Check container logs: `podman logs vanity-number-gateway`
- Look for "Configuring nginx gateway..." startup message

### Template not found error
- Ensure `/etc/nginx/nginx.conf.template` exists in the image
- Verify Dockerfile copies `gateway/nginx.conf` to the template location
- Rebuild the image if the Dockerfile was changed
