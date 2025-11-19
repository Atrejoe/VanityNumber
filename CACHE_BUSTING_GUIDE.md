# 🔄 Cache Busting Implementation Guide

## Problem Solved

**Issue**: Cloudflare (and other CDNs) cache static resources aggressively, causing users to see old versions even after deployment.

**Solution**: Timestamp-based cache busting that invalidates CDN cache on every build.

---

## How It Works

### 1. Build-Time Version Injection

Every time you build the Docker image:
1. A **Unix timestamp** is generated (e.g., `1704067200`)
2. This timestamp is injected into all static resource URLs via **inline Dockerfile commands**
3. Resources become versioned: `app.css?v=1704067200`
4. CDNs see this as a new URL and fetch fresh content

### 2. Implementation Method

**✅ Current (Inline in Dockerfile):**
- Cache busting logic is **directly in the Dockerfile**
- Uses `sed` commands to replace `{{BUILD_VERSION}}` placeholders
- No external script dependencies
- More reliable, no path issues

**❌ Old Method (External Script):**
- Used separate `apply-cache-busting.sh` file
- Required complex file path management
- Could fail if script not found or not executable

### 3. Multi-Layer Strategy

| Resource Type | Strategy | Cache Duration |
|---------------|----------|----------------|
| **HTML files** | No cache, always revalidate | 0 seconds |
| **Versioned CSS/JS** | Long cache (1 year) | 31536000 seconds |
| **Un-versioned CSS/JS** | Short cache (1 hour) | 3600 seconds |
| **Framework files** | Long cache (immutable) | 31536000 seconds |
| **Service Worker** | Versioned in query string | Varies |
| **Icons** | Versioned in query string | Varies |

### 4. Cache Headers

```nginx
# HTML - never cache
Cache-Control: no-store, no-cache, must-revalidate

# Versioned static files - cache forever
Cache-Control: public, max-age=31536000, immutable

# Un-versioned files - short cache with revalidation  
Cache-Control: public, max-age=3600, must-revalidate
```

---

## Files Modified

### 1. `VanityNumber.Web/wwwroot/index.html`
- Added `{{BUILD_VERSION}}` placeholders
- All static resources now include `?v={{BUILD_VERSION}}`
- Service Worker registration includes version

### 2. `VanityNumber.Web/Dockerfile` ✅ **Main Implementation**
- Accepts `BUILD_VERSION` build arg
- **Inline cache busting** using sed commands
- Replaces `{{BUILD_VERSION}}` in:
  - `index.html`
  - `manifest.json`
  - `service-worker.js`
- Verifies version was applied

### 3. `VanityNumber.Web/nginx.conf`
- HTML files: No cache headers
- Versioned resources: Long cache
- Un-versioned resources: Short cache with revalidation
- Cloudflare-specific bypass headers for HTML

### 4. `VanityNumber.Web/wwwroot/service-worker.js`
- Cache name includes version (updated by Dockerfile)
- Old caches are cleaned up on activation
- Network-first for HTML to always get latest

---

## Dockerfile Cache Busting Logic

The cache busting happens in this Dockerfile section:

```dockerfile
ARG BUILD_VERSION

RUN BUILD_VERSION="${BUILD_VERSION:-$(date +%s)}" && \
    echo "🔄 Applying cache busting with version: $BUILD_VERSION" && \
    WWW_ROOT="/app/publish/wwwroot" && \
    # Update index.html
    if [ -f "$WWW_ROOT/index.html" ]; then \
        sed -i "s/{{BUILD_VERSION}}/$BUILD_VERSION/g" "$WWW_ROOT/index.html"; \
    fi && \
    # Update manifest.json
    if [ -f "$WWW_ROOT/manifest.json" ]; then \
        sed -i "s/\"version\": \"[^\"]*\"/\"version\": \"$BUILD_VERSION\"/g" "$WWW_ROOT/manifest.json"; \
    fi && \
    # Update service-worker.js
    if [ -f "$WWW_ROOT/service-worker.js" ]; then \
        sed -i "s/const CACHE_NAME = 'vanity-number-v[^']*'/const CACHE_NAME = 'vanity-number-v$BUILD_VERSION'/g" "$WWW_ROOT/service-worker.js"; \
    fi
```

**Why Inline?**
- ✅ No external script file needed
- ✅ No file path issues
- ✅ Works in all build contexts
- ✅ Easy to debug
- ✅ Self-contained in Dockerfile

---

## Usage

### Building with Cache Busting

**Option A: Use Build Script (Recommended)**
```bash
# PowerShell
.\build-with-version.ps1

# Or specify tag
.\build-with-version.ps1 -ImageTag "0.0.3"

# Or use advanced script
.\Build-VanityNumberWeb.ps1 -Test -Push
```

**Option B: Manual Podman Build**
```bash
# Generate version
$BUILD_VERSION = [int][double]::Parse((Get-Date -UFormat %s))

# Build with version
podman build `
    --build-arg BUILD_VERSION="$BUILD_VERSION" `
    -t atreyu/naamnummer.web:latest `
    -f .\VanityNumber.Web\Dockerfile `
    .
```

### Testing Locally

```powershell
# Run the container
podman run -p 8080:8080 atreyu/naamnummer.web:latest

# Visit http://localhost:8080
# Check page source - you should see version numbers in URLs
```

**Verify in Browser:**
1. Open DevTools → Network tab
2. Reload page
3. Check resource URLs - should have `?v=1704067200` (timestamp)
4. Check response headers:
   - `index.html` → `Cache-Control: no-store`
   - `app.css?v=...` → `Cache-Control: public, max-age=31536000`

### Deploying to Production

```powershell
# 1. Build with version
.\build-with-version.ps1

# 2. Push to registry
podman push atreyu/naamnummer.web:latest

# 3. Deploy to Kubernetes
kubectl rollout restart deployment/vanity-robertsirre-nl `
    -n vanity-robertsirre-nl

# 4. Verify
kubectl logs -n vanity-robertsirre-nl -l app=vanity-robertsirre-nl --tail=50
```

---

## How Version Numbers Work

### Example Timeline

**Build 1 (Monday):**
```
BUILD_VERSION=1704067200
```
Generated URLs:
```
/css/app.css?v=1704067200
/js/theme-manager.js?v=1704067200
/service-worker.js?v=1704067200
```

**Build 2 (Tuesday):**
```
BUILD_VERSION=1704153600
```
Generated URLs:
```
/css/app.css?v=1704153600    ← New URL!
/js/theme-manager.js?v=1704153600    ← New URL!
/service-worker.js?v=1704153600    ← New URL!
```

**Result**: CDN sees these as completely new URLs and fetches fresh content!

---

## Verification Checklist

After deploying a new version:

- [ ] **Check HTML source**: Version numbers present
  ```bash
  curl https://vanity.robertsirre.nl/ | grep "v="
  # Should show: href="css/app.css?v=1704067200"
  ```

- [ ] **Check Cache Headers**: HTML not cached
  ```bash
  curl -I https://vanity.robertsirre.nl/
  # Should show: Cache-Control: no-store, no-cache
  ```

- [ ] **Check Static Resources**: Versioned and cached
  ```bash
  curl -I "https://vanity.robertsirre.nl/css/app.css?v=1704067200"
  # Should show: Cache-Control: public, max-age=31536000, immutable
  ```

- [ ] **Browser Test**: Hard refresh (Ctrl+Shift+R)
  - New version loads
  - No old CSS
  - Theme selector works

- [ ] **Service Worker**: New version registered
  - Open DevTools → Application → Service Workers
  - Should show new cache version

---

## Troubleshooting

### Issue: Old CSS still loading

**Cause**: Cloudflare still serving cached version

**Solutions:**
1. **Wait 5 minutes** - Cache may be propagating
2. **Hard refresh**: Ctrl+Shift+R (bypasses cache)
3. **Purge Cloudflare cache**
4. **Check HTML source**: Verify version numbers present

### Issue: Version numbers not in HTML

**Cause**: Build arg not passed or sed command failed

**Check:**
```powershell
# Inspect container
podman run --rm -it atreyu/naamnummer.web:latest sh
cat /usr/share/nginx/html/index.html | grep "v="

# Should see version numbers, not {{BUILD_VERSION}}
```

**Fix**: Check build output for "✅ Cache busting applied successfully"

### Issue: Service Worker not updating

**Cause**: Service Worker cached by browser

**Fix:**
```javascript
// In browser console:
navigator.serviceWorker.getRegistrations().then(regs => 
    regs.forEach(reg => reg.unregister())
);

// Then hard refresh
location.reload(true);
```

---

## Advantages of Inline Implementation

| Aspect | External Script | Inline Dockerfile |
|--------|----------------|-------------------|
| **Reliability** | ⚠️ Can fail if script missing | ✅ Always works |
| **Portability** | ⚠️ Requires script file | ✅ Self-contained |
| **Debugging** | ❌ Hard to see what failed | ✅ Clear in build output |
| **Maintenance** | ⚠️ Two files to maintain | ✅ One Dockerfile |
| **Path Issues** | ❌ Common problem | ✅ No path issues |

---

## Summary

### What Changed (New Implementation)

✅ **Cache busting is now inline in Dockerfile**  
✅ **No external script dependencies**  
✅ **Uses sed commands directly**  
✅ **More reliable and easier to maintain**  
✅ **Automatic timestamp generation**  
✅ **Self-contained in Dockerfile**

### What Stays the Same

✅ **Build process** - Just use `build-with-version.ps1`  
✅ **Deployment** - Same `kubectl` commands  
✅ **Performance** - Same load times  
✅ **User experience** - Better, no more stale cache!

### Best Practices

1. ✅ **Always use build script**: `.\build-with-version.ps1`
2. ✅ **Tag images with version**: Helps track deployments
3. ✅ **Test locally first**: Before pushing to production
4. ✅ **Verify version in HTML**: Check source after deploy
5. ✅ **Monitor Cloudflare**: Watch cache hit rates

---

## Quick Reference

### Build & Deploy
```powershell
# Build with cache busting
.\build-with-version.ps1

# Test locally
podman run -p 8080:8080 atreyu/naamnummer.web:latest

# Deploy
podman push atreyu/naamnummer.web:latest
kubectl rollout restart deployment/vanity-robertsirre-nl -n vanity-robertsirre-nl
```

### Verify
```powershell
# Check version in HTML
curl https://vanity.robertsirre.nl/ | grep "v="

# Check cache headers
curl -I https://vanity.robertsirre.nl/

# Check build version in container
kubectl exec -n vanity-robertsirre-nl <pod> -- grep "build-version" /usr/share/nginx/html/index.html
```

---

**Status**: ✅ Implemented (Inline in Dockerfile)  
**Method**: sed commands in RUN instruction  
**Cache Strategy**: Timestamp-based versioning  
**CDN Compatible**: Yes (Cloudflare, CloudFront, etc.)  
**No External Dependencies**: Self-contained
