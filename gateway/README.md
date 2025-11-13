# Gateway Implementation: Nginx vs YARP

## Decision: Use Nginx

We chose nginx over a .NET YARP application for the reverse proxy gateway for the following reasons:

## Comparison

| Aspect | Nginx | YARP (.NET) |
|--------|-------|-------------|
| **Image Size** | ~5MB (nginx:alpine) | ~200MB (.NET runtime + app) |
| **Memory Usage** | ~10-20MB | ~50-100MB |
| **Startup Time** | <1 second | ~2-5 seconds |
| **Configuration** | nginx.conf (declarative) | appsettings.json + C# code |
| **Ecosystem** | Battle-tested, 20+ years | Relatively new (Microsoft) |
| **Performance** | Native C, extremely fast | Excellent, but .NET overhead |
| **Simplicity** | Pure configuration | Requires .NET project |
| **Maintenance** | No code to maintain | Need to update .NET versions |
| **Build Time** | None (use official image) | ~30-60 seconds |
| **Learning Curve** | nginx config (well-known) | YARP API + .NET concepts |

## Benefits of Nginx Approach

### 1. **Simplicity**
- No custom code to write or maintain
- Pure configuration-based approach
- Standard nginx:alpine image from Docker Hub
- No build step required for Kubernetes (just ConfigMap)

### 2. **Resource Efficiency**
- Tiny footprint: 5MB vs 200MB
- Low memory usage: 10-20MB vs 50-100MB
- Fast startup: subsecond vs multiple seconds
- Lower CPU usage for the same workload

### 3. **Operational Benefits**
- No .NET runtime version management
- No security patches for custom code
- Standard image = better security scanning
- Well-documented and understood by ops teams

### 4. **Cost Savings**
In Kubernetes with 2 gateway replicas:
- **Nginx**: ~40MB memory total
- **YARP**: ~200MB memory total
- **Savings**: 160MB = potential to run 4x more pods in same cluster

### 5. **Performance**
- Nginx is written in C and highly optimized for proxy workloads
- Handles 10,000+ concurrent connections easily
- Industry standard for reverse proxying
- Used by Netflix, Airbnb, WordPress.com, etc.

## When YARP Would Be Better

YARP makes sense when you need:

1. **Complex routing logic**: Dynamic routing based on claims, headers, custom logic
2. **Request/response transformation**: Modifying requests in C# code
3. **.NET integration**: Sharing services/code with other .NET apps
4. **Custom middleware**: Complex authentication, authorization, or telemetry
5. **Service discovery**: Dynamic backend discovery using .NET libraries

## Our Use Case

We have a simple requirement:
- Route `/api/*` → API service
- Route `/swagger/*` → API service  
- Route `/*` → Web service

This is **perfect for nginx** - just basic path-based routing with no custom logic needed.

## Implementation Files

### Docker Compose
- `gateway/nginx.conf`: Nginx configuration with Docker service names
- `gateway/Dockerfile`: Simple Dockerfile copying config to nginx:alpine

### Kubernetes
- `Kubernetes/kubernetes-configmap-gateway.yaml`: Nginx config as ConfigMap
- `Kubernetes/kubernetes-deployment-gateway.yaml`: Deployment using nginx:alpine
- `Kubernetes/kubernetes-service-gateway.yaml`: LoadBalancer service

## Configuration Highlights

```nginx
# Simple upstream definitions
upstream api_backend {
    server vanity-api:8080;  # or vanity-robertsirre-nl-api:8080 in k8s
}

upstream web_backend {
    server vanity-web:8080;  # or vanity-robertsirre-nl-web:8080 in k8s
}

# Path-based routing
location /api/ {
    proxy_pass http://api_backend;
}

location /swagger/ {
    proxy_pass http://api_backend;
}

location / {
    proxy_pass http://web_backend;
}
```

Clean, simple, and easy to understand!

## Conclusion

For our straightforward reverse proxy needs, nginx provides:
- ✅ Simpler implementation
- ✅ Better resource efficiency
- ✅ Faster performance
- ✅ Lower operational overhead
- ✅ Industry-standard solution

The only "downside" is learning nginx configuration syntax, but this is valuable knowledge applicable to many projects.
