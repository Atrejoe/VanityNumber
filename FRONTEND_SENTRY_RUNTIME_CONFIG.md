# ✅ Runtime Front-End Sentry Configuration Complete!

## Summary

I've successfully implemented **runtime Sentry configuration for the front-end** using Kubernetes Secrets, eliminating the need to rebuild the Docker image when changing the Sentry DSN.

## 🎯 What Changed

### 1. **Runtime Configuration for Front-End** ✨ NEW

**Before**: Front-end Sentry DSN required rebuild
```json
// Had to edit appsettings.Production.json and rebuild
{
  "Sentry": {
    "Dsn": "https://hardcoded-dsn..."
  }
}
```

**After**: Front-end Sentry DSN configurable at runtime
```bash
# Just update secret and restart pods - no rebuild!
kubectl create secret generic vanity-sentry-secrets \
  --from-literal=web-dsn='new-dsn' \
  --dry-run=client -o yaml | kubectl apply -f -

kubectl rollout restart deployment/vanity-web
```

### 2. **Updated Files**

#### Application Files
- ✅ **`VanityNumber.Web/update-config.sh`** - Enhanced startup script
  - Reads `SENTRY_WEB_DSN` environment variable
  - Generates `appsettings.json` at runtime
  - Injects Sentry configuration dynamically
  - Supports all Sentry settings

#### Kubernetes Resources (`/Kubernetes`)
- ✅ **`kubernetes-sentry.yaml`** - Added `web-dsn` secret key
- ✅ **`kubernetes-web-deployment.yaml`** - New Web deployment (NEW FILE)
  - Configures Web environment variables
  - Injects Sentry DSN from Secret
  - 2 replicas with anti-affinity
  - Resource limits and health probes
- ✅ **`kubernetes-deployment.yaml`** - Updated API deployment
  - Added component labels
  - Created ClusterIP service for API
- ✅ **`setup-sentry-secrets.sh`** - Updated to handle both DSNs
- ✅ **`setup-sentry-secrets.ps1`** - Updated to handle both DSNs
- ✅ **`KUBERNETES.md`** - Comprehensive update

#### Docker Compose
- ✅ **`docker-compose.yml`** - Added Web Sentry env vars

## 🔐 Configuration Architecture

### Kubernetes Secrets

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: vanity-sentry-secrets
stringData:
  api-dsn: "https://api-dsn@sentry.io/xxx"     # Backend API DSN
  web-dsn: "https://web-dsn@sentry.io/yyy"     # Front-end Web DSN
```

### ConfigMap

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: vanity-sentry-config
data:
  # API settings
  api-environment: "Production"
  api-traces-sample-rate: "0.1"
  # ... other API settings
  
  # Web settings (NEW)
  web-environment: "Production"
  web-traces-sample-rate: "0.1"
  web-debug: "false"
```

### Environment Variable Injection

**API Container**:
```yaml
env:
- name: Sentry__Dsn
  valueFrom:
    secretKeyRef:
      name: vanity-sentry-secrets
      key: api-dsn
      optional: true
```

**Web Container** ✨ NEW:
```yaml
env:
- name: SENTRY_WEB_DSN
  valueFrom:
    secretKeyRef:
      name: vanity-sentry-secrets
      key: web-dsn
      optional: true

- name: SENTRY_WEB_ENVIRONMENT
  valueFrom:
    configMapKeyRef:
      name: vanity-sentry-config
      key: web-environment
      optional: true
```

## 🚀 Deployment Workflow

### Initial Setup

```bash
# 1. Create namespace
kubectl apply -f Kubernetes/kubernetes-namespace.yaml

# 2. Setup Sentry secrets (interactive - prompts for both DSNs)
cd Kubernetes
./setup-sentry-secrets.sh

# You'll be prompted for:
#   - Backend API Sentry DSN
#   - Front-end Web Sentry DSN
#   - Environment (Production)
#   - Sample Rate (0.1)

# 3. Deploy applications
kubectl apply -f Kubernetes/kubernetes-deployment.yaml
kubectl apply -f Kubernetes/kubernetes-web-deployment.yaml
kubectl apply -f Kubernetes/kubernetes-service.yaml
```

### Update Web DSN (No Rebuild Required!)

```bash
# Update the secret
kubectl create secret generic vanity-sentry-secrets \
  --namespace=vanity-robertsirre-nl \
  --from-literal=api-dsn='existing-api-dsn' \
  --from-literal=web-dsn='NEW-web-dsn' \
  --dry-run=client -o yaml | kubectl apply -f -

# Restart Web pods to pick up new configuration
kubectl rollout restart deployment/vanity-web -n vanity-robertsirre-nl

# That's it! No Docker rebuild needed!
```

## 📊 How It Works

### Web Container Startup Sequence

1. **Container Starts** → `update-config.sh` runs
2. **Read Environment Variables**:
   - `SENTRY_WEB_DSN` (from Secret)
   - `SENTRY_WEB_ENVIRONMENT` (from ConfigMap)
   - `SENTRY_WEB_SAMPLE_RATE` (from ConfigMap)
   - `SENTRY_WEB_DEBUG` (from ConfigMap)
3. **Generate `appsettings.json`**:
```json
{
  "ApiBaseUrl": "/",
  "Sentry": {
    "Dsn": "<value-from-secret>",
    "Environment": "Production",
    "TracesSampleRate": 0.1,
    "Debug": false
  }
}
```
4. **Start nginx** with updated configuration
5. **Blazor WASM** reads `appsettings.json` and initializes Sentry

### Verification

```bash
# Check generated appsettings.json in Web pod
kubectl exec -it <web-pod> -n vanity-robertsirre-nl -- \
  cat /usr/share/nginx/html/appsettings.json

# Expected output:
# {
#   "ApiBaseUrl": "/",
#   "Sentry": {
#     "Dsn": "https://your-web-dsn...",
#     "Environment": "Production",
#     "TracesSampleRate": 0.1,
#     "Debug": false
#   }
# }
```

## 🎯 Benefits

### ✅ Runtime Configuration
- No rebuild required for DSN changes
- Update secret → restart pods → done!
- Same image works in all environments

### ✅ Separate DSNs
- Backend API: `api-dsn` (private)
- Front-end Web: `web-dsn` (public)
- Better organization in Sentry
- Separate quotas and sampling

### ✅ Truly Optional
- Empty DSN → application starts normally
- No errors or warnings
- All environment variables marked `optional: true`

### ✅ Production Ready
- Anti-affinity for high availability
- Resource limits and requests
- Health probes configured
- Security context enforced

## 📁 New Deployment Structure

```
Kubernetes/
├── kubernetes-namespace.yaml          # Namespace
├── kubernetes-sentry.yaml            # Secrets & ConfigMap (updated)
├── kubernetes-deployment.yaml         # API deployment (updated)
├── kubernetes-web-deployment.yaml     # Web deployment (NEW)
├── kubernetes-service.yaml           # LoadBalancer
├── setup-sentry-secrets.sh           # Setup script (updated)
├── setup-sentry-secrets.ps1          # PowerShell script (updated)
└── KUBERNETES.md                     # Documentation (updated)
```

## 🧪 Testing

### Test Without Sentry

```bash
# Don't create secret, just deploy
kubectl apply -f kubernetes-deployment.yaml
kubectl apply -f kubernetes-web-deployment.yaml

# Check logs - both should start normally
kubectl logs -l component=api -n vanity-robertsirre-nl --tail=20
kubectl logs -l component=web -n vanity-robertsirre-nl --tail=20

# Expected: "Sentry DSN not provided - monitoring will be disabled"
```

### Test With Sentry

```bash
# Create secret with both DSNs
./setup-sentry-secrets.sh

# Deploy
kubectl apply -f kubernetes-deployment.yaml
kubectl apply -f kubernetes-web-deployment.yaml

# Check API logs
kubectl logs -l component=api -n vanity-robertsirre-nl | grep -i sentry

# Check Web configuration
POD=$(kubectl get pod -l component=web -n vanity-robertsirre-nl -o jsonpath='{.items[0].metadata.name}')
kubectl exec -it $POD -n vanity-robertsirre-nl -- cat /usr/share/nginx/html/appsettings.json

# Trigger test error in browser
# Check Sentry dashboard for events
```

### Test DSN Update

```bash
# Update Web DSN
kubectl create secret generic vanity-sentry-secrets \
  --from-literal=web-dsn='new-dsn' \
  --dry-run=client -o yaml | kubectl apply -f -

# Restart pods
kubectl rollout restart deployment/vanity-web -n vanity-robertsirre-nl

# Verify new configuration
kubectl exec -it <web-pod> -n vanity-robertsirre-nl -- \
  cat /usr/share/nginx/html/appsettings.json

# New DSN should be there!
```

## 📖 Documentation

| Document | Purpose |
|----------|---------|
| `Kubernetes/KUBERNETES.md` | Complete deployment guide |
| `Kubernetes/kubernetes-web-deployment.yaml` | Web deployment manifest |
| `VanityNumber.Web/update-config.sh` | Runtime config script |
| `docker-compose.yml` | Docker Compose with Web Sentry |

## ✅ Comparison: Before vs After

### Before (Build-Time Configuration)

```
1. Edit appsettings.Production.json
2. Rebuild Docker image
3. Push to registry
4. Update Kubernetes deployment
5. Roll out new image

Time: 5-10 minutes
Complexity: High
```

### After (Runtime Configuration) ✨

```
1. Update Kubernetes Secret
2. Restart pods

Time: < 1 minute
Complexity: Low
```

## 🎉 What You Can Do Now

### Change Web Sentry DSN Without Rebuild

```bash
kubectl create secret generic vanity-sentry-secrets \
  --namespace=vanity-robertsirre-nl \
  --from-literal=web-dsn='new-dsn' \
  --dry-run=client -o yaml | kubectl apply -f -

kubectl rollout restart deployment/vanity-web -n vanity-robertsirre-nl
```

### Use Same Image in Multiple Environments

```bash
# Staging
kubectl create secret generic vanity-sentry-secrets \
  --namespace=staging \
  --from-literal=web-dsn='staging-dsn'

# Production
kubectl create secret generic vanity-sentry-secrets \
  --namespace=production \
  --from-literal=web-dsn='production-dsn'

# Same image, different configuration!
```

### Test Different Sample Rates

```bash
# Try 100% sampling in staging
kubectl create configmap vanity-sentry-config \
  --namespace=staging \
  --from-literal=web-traces-sample-rate='1.0'

# Use 5% sampling in production
kubectl create configmap vanity-sentry-config \
  --namespace=production \
  --from-literal=web-traces-sample-rate='0.05'
```

## 🆘 Quick Reference

### Check Web Sentry Configuration

```bash
# View secret
kubectl get secret vanity-sentry-secrets -n vanity-robertsirre-nl -o yaml

# Decode Web DSN
kubectl get secret vanity-sentry-secrets -n vanity-robertsirre-nl \
  -o jsonpath='{.data.web-dsn}' | base64 -d

# View generated config
kubectl exec -it <web-pod> -n vanity-robertsirre-nl -- \
  cat /usr/share/nginx/html/appsettings.json

# Check environment variables
kubectl exec -it <web-pod> -n vanity-robertsirre-nl -- env | grep SENTRY
```

### Restart Components

```bash
# Restart API
kubectl rollout restart deployment/vanity-api -n vanity-robertsirre-nl

# Restart Web
kubectl rollout restart deployment/vanity-web -n vanity-robertsirre-nl

# Restart both
kubectl rollout restart deployment/vanity-api deployment/vanity-web -n vanity-robertsirre-nl
```

---

**Status**: ✅ Complete  
**Build**: ✅ Successful  
**Runtime Config**: ✅ Implemented  
**No Rebuild Required**: ✅ Both DSNs configurable at runtime  
**Separate DSNs**: ✅ API and Web use different Sentry projects  
**Truly Optional**: ✅ Empty DSN = normal startup  

**Deploy Now**: `cd Kubernetes && ./setup-sentry-secrets.sh`
