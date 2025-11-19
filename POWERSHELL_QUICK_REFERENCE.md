# 🪟 PowerShell Build Scripts - Quick Reference

## ✅ Created Scripts

### 1. `build-with-version.ps1`
**Purpose**: Simple build with cache busting  
**Best for**: Quick builds during development

```powershell
# Basic usage
.\build-with-version.ps1

# With custom tag
.\build-with-version.ps1 -ImageTag "0.0.3"
```

### 2. `Build-VanityNumberWeb.ps1`
**Purpose**: Full-featured build with all options  
**Best for**: Production builds, CI/CD pipelines

```powershell
# Just build
.\Build-VanityNumberWeb.ps1

# Build and test
.\Build-VanityNumberWeb.ps1 -Test

# Build, test, and push
.\Build-VanityNumberWeb.ps1 -Test -Push

# Full rebuild (no cache)
.\Build-VanityNumberWeb.ps1 -NoBuildCache -Test -Push
```

### 3. `Quick-Build.ps1`
**Purpose**: Common workflows in one command  
**Best for**: Daily development tasks

```powershell
# Just build
.\Quick-Build.ps1 -Action build

# Build and test
.\Quick-Build.ps1 -Action test

# Build, test, push, and deploy
.\Quick-Build.ps1 -Action deploy

# Full rebuild and deploy
.\Quick-Build.ps1 -Action full
```

## 🎯 Common Scenarios

### Scenario 1: Local Development

**Goal**: Build and test locally

```powershell
.\Build-VanityNumberWeb.ps1 -Test
```

**What it does:**
1. Builds container with cache busting
2. Starts container on port 8080
3. Tests health endpoint
4. Stops and removes container

### Scenario 2: Deploy to Production

**Goal**: Build, test, and deploy

```powershell
.\Quick-Build.ps1 -Action deploy
```

**What it does:**
1. Builds with cache busting
2. Tests container
3. Pushes to registry
4. Deploys to Kubernetes
5. Waits for rollout completion

### Scenario 3: Emergency Fix

**Goal**: Force complete rebuild

```powershell
.\Build-VanityNumberWeb.ps1 -NoBuildCache -Test -Push
kubectl rollout restart deployment/vanity-robertsirre-nl -n vanity-robertsirre-nl
```

**What it does:**
1. Rebuilds without cache (fresh build)
2. Tests thoroughly
3. Pushes to registry
4. Restarts deployment

### Scenario 4: Different Registry

**Goal**: Push to custom registry

```powershell
.\Build-VanityNumberWeb.ps1 `
    -Registry "myregistry.io/myuser" `
    -ImageTag "v1.0.0" `
    -Push
```

## 📋 Script Comparison

| Feature | Simple | Advanced | Quick |
|---------|--------|----------|-------|
| **File** | `build-with-version.ps1` | `Build-VanityNumberWeb.ps1` | `Quick-Build.ps1` |
| Basic build | ✅ | ✅ | ✅ |
| Automatic testing | ❌ | ✅ (optional) | ✅ |
| Automatic push | ❌ | ✅ (optional) | ✅ |
| Kubernetes deploy | ❌ | ❌ | ✅ |
| No cache option | ❌ | ✅ | ✅ |
| Custom registry | ❌ | ✅ | ❌ |
| Workflow presets | ❌ | ❌ | ✅ |
| **Best for** | Quick dev | Production | Daily tasks |

## 🔧 Parameters Reference

### `build-with-version.ps1`
```powershell
-ImageTag "latest"    # Docker image tag
```

### `Build-VanityNumberWeb.ps1`
```powershell
-ImageTag "latest"        # Docker image tag
-Registry "atreyu"        # Registry name
-Push                     # Push to registry after build
-Test                     # Test container after build
-NoBuildCache            # Build without cache
-BuildVersion "123456"   # Custom build version
```

### `Quick-Build.ps1`
```powershell
-Action "build"     # Actions: build, test, deploy, full
-Tag "latest"       # Docker image tag
```

## ⚡ Quick Commands

### Daily Development
```powershell
# Morning: Build and test
.\Quick-Build.ps1 -Action test

# After changes: Quick build
.\build-with-version.ps1

# Ready to deploy: Full workflow
.\Quick-Build.ps1 -Action deploy
```

### Production Deployment
```powershell
# Standard deployment
.\Build-VanityNumberWeb.ps1 -Test -Push
kubectl rollout restart deployment/vanity-robertsirre-nl -n vanity-robertsirre-nl

# Or use Quick-Build
.\Quick-Build.ps1 -Action deploy
```

### Troubleshooting Build Issues
```powershell
# Full rebuild without cache
.\Build-VanityNumberWeb.ps1 -NoBuildCache -Test

# Check container logs
kubectl logs -n vanity-robertsirre-nl -l app=vanity-robertsirre-nl --tail=50

# Test locally
podman run -p 8080:8080 atreyu/naamnummer.web:latest
# Visit http://localhost:8080
```

## 🐛 Common Issues

### Issue 1: Execution Policy Error

**Error:**
```
.\Build-VanityNumberWeb.ps1 : File cannot be loaded
```

**Fix:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Issue 2: Podman Not Found

**Error:**
```
❌ Podman is not installed or not in PATH
```

**Fix:**
1. Install from https://podman-desktop.io/downloads/windows
2. Restart PowerShell
3. Verify: `podman --version`

### Issue 3: Push Failed - Unauthorized

**Error:**
```
❌ Push failed: unauthorized
```

**Fix:**
```powershell
podman login docker.io
# Enter username and password
```

### Issue 4: Kubernetes Not Connected

**Error:**
```
error: You must be logged in to the server
```

**Fix:**
```powershell
# Configure kubectl
kubectl config get-contexts
kubectl config use-context <your-context>
```

## 📊 Build Output Explained

```
╔════════════════════════════════════════════════════════════╗
║  Vanity Number Web - Container Build Script (Podman)      ║
╚════════════════════════════════════════════════════════════╝

📋 Build Configuration:
   Build Version:  1704067200          ← Timestamp for cache busting
   Image Name:     atreyu/naamnummer.web
   Image Tag:      latest
   Dockerfile:     .\VanityNumber.Web\Dockerfile
   No Cache:       False
   Auto Push:      True
   Auto Test:      True

✓ Podman detected: podman version 4.8.0    ← Podman installed correctly

🏗️  Building container image...

STEP 1/20: FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
...
[Build steps...]
...
✅ Build completed successfully!

📦 Images created:
   atreyu/naamnummer.web:latest          ← Can use this
   atreyu/naamnummer.web:v1704067200    ← Or this (versioned)

🧪 Testing container...
✅ Container is running and healthy!       ← Tests passed!

🚀 Pushing images to registry...
✅ Images pushed successfully!             ← Ready to deploy

📋 Build Version: 1704067200              ← For tracking
```

## 🎯 Best Practices

### For Development
1. **Use test flag**: `.\Build-VanityNumberWeb.ps1 -Test`
2. **Test locally first** before pushing
3. **Use Quick-Build** for common tasks

### For Production
1. **Always test**: Include `-Test` flag
2. **Use versioned tags**: `-ImageTag "v1.0.0"`
3. **Verify in browser** after deployment
4. **Monitor logs**: Check Kubernetes logs

### For CI/CD
1. **Use `-NoBuildCache`** for clean builds
2. **Automate testing**: Always include `-Test`
3. **Tag with version**: Use semantic versioning
4. **Log build version**: Track deployments

## 📚 Additional Resources

- **Complete Guide**: `POWERSHELL_BUILD_GUIDE.md`
- **Cache Busting**: `CACHE_BUSTING_GUIDE.md`
- **Deployment Troubleshooting**: `BLAZOR_DEPLOYMENT_TROUBLESHOOTING.md`

## ✅ Quick Start Checklist

- [ ] Install Podman Desktop
- [ ] Set PowerShell execution policy
- [ ] Login to container registry: `podman login`
- [ ] Build and test: `.\Build-VanityNumberWeb.ps1 -Test`
- [ ] Verify at http://localhost:8080
- [ ] Push to registry: `-Push` flag
- [ ] Deploy to Kubernetes

## 🎉 You're Ready!

Choose your script based on what you need:
- **Quick dev work?** → `build-with-version.ps1`
- **Production build?** → `Build-VanityNumberWeb.ps1 -Test -Push`
- **Common workflow?** → `Quick-Build.ps1 -Action deploy`

---

**All scripts use Podman and correct Dockerfile path!**  
**Cache busting is automatic!**  
**Ready to deploy to vanity.robertsirre.nl!**
