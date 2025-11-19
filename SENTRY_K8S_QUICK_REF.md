# Sentry Kubernetes Quick Reference

## 🚀 Deploy in 3 Commands

```bash
# 1. Create namespace
kubectl apply -f Kubernetes/kubernetes-namespace.yaml

# 2. Setup Sentry (interactive)
cd Kubernetes && ./setup-sentry-secrets.sh

# 3. Deploy application
kubectl apply -f Kubernetes/kubernetes-deployment.yaml,Kubernetes/kubernetes-service.yaml
```

## 🔐 DSN Configuration

### Backend API (Kubernetes Secret)
```bash
kubectl create secret generic vanity-sentry-secrets \
  --namespace=vanity-robertsirre-nl \
  --from-literal=api-dsn='https://api-dsn@sentry.io/xxx'
```

### Front-End (Build Time)
```json
// VanityNumber.Web/wwwroot/appsettings.Production.json
{
  "Sentry": {
    "Dsn": "https://web-dsn@sentry.io/xxx"
  }
}
```

## 📊 Verify

```bash
# Check pods
kubectl get pods -n vanity-robertsirre-nl

# View logs
kubectl logs -l app=vanity-robertsirre-nl -n vanity-robertsirre-nl --tail=50

# Check Sentry config
kubectl exec -it <pod> -n vanity-robertsirre-nl -- env | grep Sentry
```

## 🔄 Update

```bash
# Update DSN
kubectl create secret generic vanity-sentry-secrets \
  --from-literal=api-dsn='new-dsn' \
  --dry-run=client -o yaml | kubectl apply -f -

# Restart
kubectl rollout restart deployment/vanity-robertsirre-nl -n vanity-robertsirre-nl
```

## 🐛 Troubleshoot

```bash
# View secret
kubectl get secret vanity-sentry-secrets -n vanity-robertsirre-nl -o yaml

# Decode DSN
kubectl get secret vanity-sentry-secrets -n vanity-robertsirre-nl \
  -o jsonpath='{.data.api-dsn}' | base64 -d

# Pod events
kubectl describe pod -l app=vanity-robertsirre-nl -n vanity-robertsirre-nl
```

## ℹ️ Important

✅ **Separate DSNs**: Use different Sentry projects for API and Web  
✅ **Optional**: Empty DSN = app starts normally  
✅ **Early Init**: API captures startup errors  
✅ **Build Time**: Front-end DSN set before Docker build  

## 📖 Full Docs

- `Kubernetes/KUBERNETES.md` - Complete guide
- `SENTRY_MONITORING_GUIDE.md` - Sentry details
- `SENTRY_KUBERNETES_UPDATE.md` - Implementation summary
