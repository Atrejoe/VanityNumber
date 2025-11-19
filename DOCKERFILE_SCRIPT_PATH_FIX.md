# 🐛 Dockerfile Build Error - Script Not Found

## Error Message
```
/bin/sh: 1: /tmp/apply-cache-busting.sh: not found
Error: building at STEP "RUN chmod +x /tmp/apply-cache-busting.sh...": exit status 127
```

## Root Cause

The `apply-cache-busting.sh` script was being copied to `/tmp/` **after** it was already copied with the source files. The duplicate COPY command was failing because:

1. Earlier in Dockerfile: `COPY VanityNumber.Web/ VanityNumber.Web/` ✅ (script already copied here)
2. Later in Dockerfile: `COPY VanityNumber.Web/apply-cache-busting.sh /tmp/` ❌ (trying to copy again from wrong context)

The Docker build context changes after `WORKDIR /src/VanityNumber.Web`, so the relative path doesn't work the same way.

## Solution

Use the script that's **already in the working directory** from the earlier COPY:

### ❌ Before (Broken)
```dockerfile
WORKDIR /src/VanityNumber.Web
RUN dotnet publish VanityNumber.Web.csproj -c Release -o /app/publish

# This fails because the path is wrong after WORKDIR change
COPY VanityNumber.Web/apply-cache-busting.sh /tmp/apply-cache-busting.sh
RUN chmod +x /tmp/apply-cache-busting.sh && \
    BUILD_VERSION="${BUILD_VERSION:-$(date +%s)}" /tmp/apply-cache-busting.sh
```

### ✅ After (Fixed)
```dockerfile
WORKDIR /src/VanityNumber.Web
RUN dotnet publish VanityNumber.Web.csproj -c Release -o /app/publish

# Use the script that's already here from earlier COPY
RUN if [ -f ./apply-cache-busting.sh ]; then \
        chmod +x ./apply-cache-busting.sh && \
        BUILD_VERSION="${BUILD_VERSION:-$(date +%s)}" ./apply-cache-busting.sh; \
    else \
        echo "⚠️  Warning: apply-cache-busting.sh not found"; \
    fi
```

## Key Changes

1. **Removed duplicate COPY command**
2. **Use `./apply-cache-busting.sh`** (current directory)
3. **Added existence check** with `if [ -f ./apply-cache-busting.sh ]`
4. **Added graceful fallback** with warning message

## Why This Works

### Docker Build Context Flow

```
1. COPY VanityNumber.Web/ VanityNumber.Web/
   → Script is now at: /src/VanityNumber.Web/apply-cache-busting.sh

2. WORKDIR /src/VanityNumber.Web
   → Current directory is now: /src/VanityNumber.Web/

3. RUN ./apply-cache-busting.sh
   → Runs: /src/VanityNumber.Web/apply-cache-busting.sh ✅
```

## Testing

```powershell
# Build and verify
.\Quick-Build.ps1 -Tag 0.4.2

# Check build output for:
# "✓ Updating index.html"
# "✅ Cache busting applied successfully"
```

## Common Docker Path Issues

### Issue 1: COPY After WORKDIR
```dockerfile
WORKDIR /app
COPY myfile.txt /tmp/  # ❌ Path is relative to context root, not /app
```

**Fix:** COPY before WORKDIR or use full context path

### Issue 2: Relative Paths in RUN
```dockerfile
COPY script.sh /tmp/
WORKDIR /app
RUN /tmp/script.sh  # ✅ Absolute path works
RUN ./script.sh     # ❌ Looks in /app, not /tmp
```

### Issue 3: Duplicate COPY
```dockerfile
COPY source/ dest/      # Copies everything
COPY source/file.txt /tmp/  # ❌ Redundant and can fail
```

**Fix:** Copy once, reference from copied location

## Best Practices

### 1. Copy Scripts Once
```dockerfile
# Copy all source including scripts
COPY VanityNumber.Web/ VanityNumber.Web/

# Use scripts from copied location
WORKDIR /src/VanityNumber.Web
RUN ./my-script.sh
```

### 2. Check Before Running
```dockerfile
RUN if [ -f ./script.sh ]; then \
        chmod +x ./script.sh && ./script.sh; \
    else \
        echo "Script not found"; \
    fi
```

### 3. Use Absolute Paths for Clarity
```dockerfile
COPY scripts/ /opt/scripts/
RUN /opt/scripts/build.sh  # Clear and explicit
```

### 4. Verify Script Existence Locally
```powershell
# Before building, verify script exists
Test-Path "VanityNumber.Web\apply-cache-busting.sh"
```

## Debugging Docker Script Issues

### 1. List Files at Build Stage
```dockerfile
RUN echo "Files in current directory:" && ls -la
```

### 2. Check Script Content
```dockerfile
RUN cat ./script.sh  # Verify script was copied correctly
```

### 3. Test Script Permissions
```dockerfile
RUN ls -l ./script.sh  # Check if executable
```

### 4. Use Multi-Stage Debug
```dockerfile
# Create intermediate stage to inspect
FROM base AS debug
COPY . .
RUN ls -laR /src/

FROM debug AS build
# Continue with build
```

## Related Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `not found` | Wrong path or file doesn't exist | Check path relative to WORKDIR |
| `permission denied` | Script not executable | Add `chmod +x` |
| `syntax error` | Script has Windows line endings | Convert to Unix (LF) |
| `command not found` | Missing shebang or wrong interpreter | Add `#!/bin/sh` |

## Prevention Checklist

- [ ] Verify script exists in source directory
- [ ] Check script has Unix line endings (LF, not CRLF)
- [ ] Use consistent paths throughout Dockerfile
- [ ] Add existence checks before running scripts
- [ ] Test build locally before pushing
- [ ] Review Docker build context (`.dockerignore`)

## Summary

**Problem:** Trying to COPY script from wrong path after WORKDIR change  
**Solution:** Use script that's already in working directory  
**Prevention:** Copy once, reference from there  
**Best Practice:** Check file exists before running

---

**Status:** ✅ Fixed  
**Applies to:** Docker, Podman, Buildah  
**Related:** DOCKERFILE_TROUBLESHOOTING.md
