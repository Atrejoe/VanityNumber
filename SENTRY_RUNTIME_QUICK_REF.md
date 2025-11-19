# Sentry Runtime Configuration - Quick Reference

## 🚀 Quick Deploy (Both API & Web)

```bash
# 1. Create namespace
kubectl apply -f Kubernetes/kubernetes-namespace.yaml

# 2. Setup Sentry (interactive - prompts for BOTH DSNs)
cd Kubernetes && ./setup-sentry-secrets.sh

# 3. Deploy
kubectl apply -f Kubernetes/kubernetes-deployment.yaml
kubectl apply -f Kubernetes/kubernetes-web-deployment.yaml
```

## 🔐 Two Separate DSNs

| Component | DSN Key | Configuration | Visibility |
|-----------|---------|---------------|------------|
| **API** | `api-dsn` | Runtime (Secret) | Private |
| **Web** | `web-dsn` | Runtime (Secret) ✨ NEW | Public |

## 📝 Update DSNs (No Rebuild!)

### Update API DSN
```bash
kubectl create secret generic vanity-sentry-secrets \
  --namespace=vanity-robertsirre-nl \
  --from-literal=api-dsn='new-api-dsn' \
  --from-literal=web-dsn='existing-web-dsn' \
  --dry-run=client -o yaml | kubectl apply -f -

kubectl rollout restart deployment/vanity-api -n vanity-robertsirre-nl
```

### Update Web DSN ✨ NEW
```bash
kubectl create secret generic vanity-sentry-secrets \
  --namespace=vanity-robertsirre-nl \
  --from-literal=api-dsn='existing-api-dsn' \
  --from-literal=web-dsn='new-web-dsn' \
  --dry-run=client -o yaml | kubectl apply -f -

kubectl rollout restart deployment/vanity-web -n vanity-robertsirre-nl
```

## 🔍 Verify Configuration

### API
```bash
# Check environment
kubectl exec -it <api-pod> -n vanity-robertsirre-nl -- env | grep Sentry

# Check logs
kubectl logs -l component=api -n vanity-robertsirre-nl | grep -i sentry
```

### Web ✨ NEW
```bash
# Check generated config
kubectl exec -it <web-pod> -n vanity-robertsirre-nl -- \
  cat /usr/share/nginx/html/appsettings.json

# Check environment
kubectl exec -it <web-pod> -n vanity-robertsirre-nl -- env | grep SENTRY

# Check logs
kubectl logs -l component=web -n vanity-robertsirre-nl --tail=50
```

## 📊 Environment Variables

### API (Standard ASP.NET Core)
```yaml
Sentry__Dsn                # From Secret
Sentry__Environment        # From ConfigMap
Sentry__TracesSampleRate   # From ConfigMap
```

### Web (Custom Runtime) ✨ NEW
```yaml
SENTRY_WEB_DSN             # From Secret
SENTRY_WEB_ENVIRONMENT     # From ConfigMap
SENTRY_WEB_SAMPLE_RATE     # From ConfigMap
SENTRY_WEB_DEBUG           # From ConfigMap
```

## 🎯 Key Benefits

✅ **No Rebuild**: Change Web DSN without rebuilding image  
✅ **Separate DSNs**: Different projects for API and Web  
✅ **Runtime Config**: Both components configurable at runtime  
✅ **Optional**: Empty DSN = disabled (no errors)  

## 📖 Full Docs

- `Kubernetes/KUBERNETES.md` - Complete guide
- `FRONTEND_SENTRY_RUNTIME_CONFIG.md` - Implementation details
- `SENTRY_KUBERNETES_UPDATE.md` - Original Kubernetes update
