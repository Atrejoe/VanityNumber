# 🪟 PowerShell Build Scripts for Windows

This directory contains PowerShell scripts for building the Vanity Number Web application using Podman on Windows.

## 📁 Available Scripts

### 1. `build-with-version.ps1` (Simple)
Quick and simple build script with cache busting.

**Usage:**
```powershell
# Basic build
.\build-with-version.ps1

# Build with specific tag
.\build-with-version.ps1 -ImageTag "0.0.3"
```

### 2. `Build-VanityNumberWeb.ps1` (Advanced)
Full-featured build script with multiple options.

**Usage:**
```powershell
# Basic build
.\Build-VanityNumberWeb.ps1

# Build with specific tag
.\Build-VanityNumberWeb.ps1 -ImageTag "0.0.3"

# Build and test
.\Build-VanityNumberWeb.ps1 -Test

# Build and push to registry
.\Build-VanityNumberWeb.ps1 -Push

# Build without cache
.\Build-VanityNumberWeb.ps1 -NoBuildCache

# Build, test, and push
.\Build-VanityNumberWeb.ps1 -Test -Push

# Custom registry
.\Build-VanityNumberWeb.ps1 -Registry "myregistry.io/myuser"

# Custom build version
.\Build-VanityNumberWeb.ps1 -BuildVersion "20240101120000"

# All options combined
.\Build-VanityNumberWeb.ps1 `
    -ImageTag "0.0.3" `
    -Registry "atreyu" `
    -Test `
    -Push `
    -NoBuildCache
```

## 🎯 Parameters

### `build-with-version.ps1`

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ImageTag` | string | "latest" | Docker image tag |

### `Build-VanityNumberWeb.ps1`

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ImageTag` | string | "latest" | Docker image tag |
| `Registry` | string | "atreyu" | Container registry name |
| `Push` | switch | false | Automatically push to registry |
| `Test` | switch | false | Test container after build |
| `NoBuildCache` | switch | false | Build without using cache |
| `BuildVersion` | string | timestamp | Custom build version |

## 🚀 Quick Start

### Prerequisites

1. **Install Podman Desktop:**
   ```
   Download from: https://podman-desktop.io/downloads/windows
   ```

2. **Verify Podman:**
   ```powershell
   podman --version
   # Should show: podman version X.X.X
   ```

3. **Login to Registry (if pushing):**
   ```powershell
   podman login docker.io
   # Or your custom registry
   ```

### Build and Test

```powershell
# Navigate to project root
cd C:\Users\rsirre\Desktop\naamnummers

# Build with automatic testing
.\Build-VanityNumberWeb.ps1 -Test

# If successful, the container will start on http://localhost:8080
```

### Build and Deploy

```powershell
# Build, test, and push to registry
.\Build-VanityNumberWeb.ps1 -Test -Push

# Deploy to Kubernetes
kubectl set image deployment/vanity-robertsirre-nl `
    api=atreyu/naamnummer.web:latest `
    -n vanity-robertsirre-nl

# Or restart deployment
kubectl rollout restart deployment/vanity-robertsirre-nl `
    -n vanity-robertsirre-nl
```

## 🐛 Troubleshooting

### "Podman is not installed"

**Solution:**
1. Download Podman Desktop: https://podman-desktop.io/downloads/windows
2. Install and restart PowerShell
3. Verify: `podman --version`

### "Execution Policy Error"

**Error:**
```
.\Build-VanityNumberWeb.ps1 : File cannot be loaded because running scripts is disabled
```

**Solution:**
```powershell
# Set execution policy for current user
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Or run with bypass
powershell -ExecutionPolicy Bypass -File .\Build-VanityNumberWeb.ps1
```

### "Dockerfile not found"

**Solution:**
Make sure you're in the project root directory:
```powershell
cd C:\Users\rsirre\Desktop\naamnummers
# Then run the script
.\Build-VanityNumberWeb.ps1
```

### "Push failed: unauthorized"

**Solution:**
Login to your container registry:
```powershell
# Docker Hub
podman login docker.io

# Custom registry
podman login myregistry.io

# Then retry the build with -Push
.\Build-VanityNumberWeb.ps1 -Push
```

## 📊 Build Output Example

```
╔════════════════════════════════════════════════════════════╗
║  Vanity Number Web - Container Build Script (Podman)      ║
╚════════════════════════════════════════════════════════════╝

📋 Build Configuration:
   Build Version:  1704067200
   Image Name:     atreyu/naamnummer.web
   Image Tag:      latest
   Dockerfile:     .\VanityNumber.Web\Dockerfile
   No Cache:       False
   Auto Push:      True
   Auto Test:      True

✓ Podman detected: podman version 4.8.0

🏗️  Building container image...

[... build output ...]

✅ Build completed successfully!

📦 Images created:
   atreyu/naamnummer.web:latest
   atreyu/naamnummer.web:v1704067200

🧪 Testing container...
✅ Container is running and healthy!

🚀 Pushing images to registry...
✅ Images pushed successfully!

═══════════════════════════════════════════════════════════

📋 Build Version: 1704067200
```

## 🔧 Advanced Usage

### Custom Build Version Format

```powershell
# Use semantic version
$version = "1.0.0-$(Get-Date -Format 'yyyyMMddHHmmss')"
.\Build-VanityNumberWeb.ps1 -BuildVersion $version

# Use Git commit hash
$commit = git rev-parse --short HEAD
.\Build-VanityNumberWeb.ps1 -BuildVersion $commit
```

### Build for Multiple Registries

```powershell
# Build and tag for multiple registries
$registries = @("atreyu", "myregistry.io/myuser")

foreach ($registry in $registries) {
    .\Build-VanityNumberWeb.ps1 `
        -Registry $registry `
        -ImageTag "0.0.3" `
        -Push
}
```

### Automated CI/CD Pipeline

```powershell
# Example CI/CD script
param(
    [string]$Version = "latest"
)

# Build
.\Build-VanityNumberWeb.ps1 -ImageTag $Version -NoBuildCache

# Test
.\Build-VanityNumberWeb.ps1 -ImageTag $Version -Test

# If tests pass, push
if ($LASTEXITCODE -eq 0) {
    .\Build-VanityNumberWeb.ps1 -ImageTag $Version -Push
    
    # Deploy to Kubernetes
    kubectl set image deployment/vanity-robertsirre-nl `
        api=atreyu/naamnummer.web:$Version `
        -n vanity-robertsirre-nl
}
```

## 📝 Script Comparison

| Feature | `build-with-version.ps1` | `Build-VanityNumberWeb.ps1` |
|---------|--------------------------|------------------------------|
| Basic build | ✅ | ✅ |
| Custom tag | ✅ | ✅ |
| Auto test | ❌ | ✅ |
| Auto push | ❌ | ✅ |
| No cache option | ❌ | ✅ |
| Custom registry | ❌ | ✅ |
| Custom build version | ❌ | ✅ |
| Verbose output | ❌ | ✅ |
| Error handling | Basic | Advanced |
| Help text | ❌ | ✅ |

## 🎨 Using with Docker Instead of Podman

Want to use Docker instead? Just replace `podman` with `docker` in the scripts:

```powershell
# In build-with-version.ps1 or Build-VanityNumberWeb.ps1
# Change:
podman build ...

# To:
docker build ...
```

Or create an alias:
```powershell
Set-Alias -Name podman -Value docker
```

## 🆘 Getting Help

### Script Help
```powershell
# Get detailed parameter help
Get-Help .\Build-VanityNumberWeb.ps1 -Detailed

# Show examples
Get-Help .\Build-VanityNumberWeb.ps1 -Examples
```

### Podman Help
```powershell
# Podman commands
podman --help
podman build --help
podman run --help
```

## 📚 Additional Resources

- **Podman Documentation**: https://docs.podman.io/
- **Podman Desktop**: https://podman-desktop.io/
- **PowerShell Documentation**: https://docs.microsoft.com/powershell/
- **Cache Busting Guide**: See `CACHE_BUSTING_GUIDE.md`

## ✅ Checklist for First Time Use

- [ ] Install Podman Desktop
- [ ] Verify Podman is in PATH: `podman --version`
- [ ] Set PowerShell execution policy (if needed)
- [ ] Login to container registry: `podman login`
- [ ] Navigate to project root directory
- [ ] Run build script: `.\Build-VanityNumberWeb.ps1 -Test`
- [ ] Verify build version in container
- [ ] Push to registry (if ready): `-Push` flag
- [ ] Deploy to Kubernetes

## 🎉 Success!

If you see this:
```
✅ Build completed successfully!
✅ Container is running and healthy!
✅ Images pushed successfully!
```

You're ready to deploy! 🚀

---

**Created for Windows users who prefer PowerShell over Bash**

For Bash scripts, see: `build-with-version.sh`  
For complete cache busting guide, see: `CACHE_BUSTING_GUIDE.md`
