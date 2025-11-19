# 🐛 Common Docker/Podman Build Errors - Troubleshooting

## Error: "Syntax error - can't find = in '+%s)}'"

### Full Error Message
```
Error: parsing main Dockerfile: /var/tmp/libpod_builder3771596764/build/VanityNumber.Web/Dockerfile: 
Syntax error - can't find = in "+%s)}". Must be of the form: name=value
```

### Root Cause
Docker/Podman `ENV` command **does not support shell command substitution** like `$(command)`.

### ❌ Wrong Way
```dockerfile
# This FAILS:
ARG BUILD_VERSION
ENV BUILD_VERSION=${BUILD_VERSION:-$(date +%s)}
```

### ✅ Correct Way
```dockerfile
# Use RUN with shell command:
ARG BUILD_VERSION
RUN BUILD_VERSION="${BUILD_VERSION:-$(date +%s)}" /path/to/script.sh
```

### Explanation

**Docker `ENV` command:**
- Sets environment variables at **image build time**
- Values are **literal strings only**
- No shell expansion or command substitution
- Format: `ENV KEY=value` or `ENV KEY value`

**Docker `RUN` command:**
- Executes commands in a **shell**
- Shell expansion and command substitution **work**
- Can use `$(command)` syntax
- Environment variables are set **during that RUN only**

### Our Solution

**Before (broken):**
```dockerfile
ARG BUILD_VERSION
ENV BUILD_VERSION=${BUILD_VERSION:-$(date +%s)}  # ❌ FAILS
```

**After (working):**
```dockerfile
ARG BUILD_VERSION
RUN BUILD_VERSION="${BUILD_VERSION:-$(date +%s)}" /tmp/apply-cache-busting.sh  # ✅ WORKS
```

The script receives `BUILD_VERSION` as an environment variable within the RUN context.

---

## Other Common Dockerfile Syntax Errors

### 1. Multi-line ENV with Wrong Syntax

**❌ Wrong:**
```dockerfile
ENV VAR1=value1 \
    VAR2=value2 \
    VAR3=value3
```

**✅ Correct:**
```dockerfile
ENV VAR1=value1
ENV VAR2=value2
ENV VAR3=value3

# Or use spaces (not recommended for clarity):
ENV VAR1=value1 VAR2=value2 VAR3=value3
```

### 2. Using Quotes Incorrectly

**❌ Wrong:**
```dockerfile
ENV PATH="/usr/local/bin:$PATH"  # Quotes prevent expansion
```

**✅ Correct:**
```dockerfile
ENV PATH=/usr/local/bin:$PATH  # No quotes for expansion
# Or:
ENV PATH="${PATH}:/usr/local/bin"  # Braces help
```

### 3. ARG vs ENV Confusion

**ARG** - Build-time variable (only during build):
```dockerfile
ARG BUILD_VERSION=123
RUN echo "Building version: $BUILD_VERSION"
# Available in RUN commands during build
```

**ENV** - Runtime variable (in final image):
```dockerfile
ENV APP_VERSION=1.0.0
# Available when container runs
```

**Both:**
```dockerfile
ARG BUILD_VERSION
ENV BUILD_VERSION=$BUILD_VERSION
# ARG for build, ENV for runtime
```

### 4. COPY Path Issues

**❌ Wrong:**
```dockerfile
COPY ./VanityNumber.Web/Dockerfile .  # Copies Dockerfile itself
```

**✅ Correct:**
```dockerfile
COPY VanityNumber.Web/ VanityNumber.Web/  # Relative to context
```

### 5. Shell vs Exec Form

**Shell form** (with shell):
```dockerfile
RUN npm install  # Uses shell, can use pipes, etc.
CMD npm start    # Uses shell
```

**Exec form** (no shell):
```dockerfile
RUN ["npm", "install"]  # No shell, faster
CMD ["npm", "start"]    # Recommended for production
```

---

## Debugging Docker Builds

### 1. Check Syntax Before Building
```bash
# Lint Dockerfile
docker run --rm -i hadolint/hadolint < VanityNumber.Web/Dockerfile
```

### 2. Build with Verbose Output
```bash
# See every step
podman build --progress=plain -f VanityNumber.Web/Dockerfile .
```

### 3. Check Build Args
```bash
# Verify ARG is passed correctly
podman build --build-arg BUILD_VERSION=12345 -f VanityNumber.Web/Dockerfile .
```

### 4. Inspect Failed Layer
```bash
# If build fails, inspect last successful layer
podman images --all
podman run -it <last-successful-layer-id> sh
```

### 5. Test RUN Commands Locally
```bash
# Test the command in Alpine container
podman run --rm -it alpine:latest sh
# Then try your RUN command
```

---

## Quick Reference: Dockerfile Commands

| Command | Purpose | Shell Expansion | Command Substitution |
|---------|---------|-----------------|----------------------|
| `FROM` | Base image | ❌ | ❌ |
| `ARG` | Build-time variable | ❌ | ❌ |
| `ENV` | Runtime variable | ✅ (limited) | ❌ |
| `RUN` | Execute command | ✅ | ✅ |
| `COPY` | Copy files | ❌ | ❌ |
| `ADD` | Copy + extract | ❌ | ❌ |
| `WORKDIR` | Set directory | ✅ (ENV only) | ❌ |
| `CMD` | Default command | Depends on form | Depends on form |
| `ENTRYPOINT` | Entry command | Depends on form | Depends on form |

---

## Best Practices

### 1. Use ARG for Build-Time Values
```dockerfile
ARG BUILD_VERSION=latest
ARG NODE_VERSION=18
```

### 2. Use ENV for Runtime Values
```dockerfile
ENV NODE_ENV=production
ENV PORT=8080
```

### 3. Combine RUN Commands
```dockerfile
# ❌ Multiple layers:
RUN apt-get update
RUN apt-get install -y package1
RUN apt-get install -y package2

# ✅ Single layer:
RUN apt-get update && \
    apt-get install -y \
        package1 \
        package2 && \
    apt-get clean
```

### 4. Use Multi-Stage Builds
```dockerfile
# Build stage
FROM node:18 AS builder
RUN npm install && npm run build

# Runtime stage
FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html
```

### 5. Minimize Layers
- Each `RUN`, `COPY`, `ADD` creates a new layer
- Combine related commands
- Clean up in the same layer

---

## Testing Your Fix

After fixing the Dockerfile:

```powershell
# 1. Clean build (no cache)
.\Build-VanityNumberWeb.ps1 -NoBuildCache

# 2. Check build output for errors
# Look for: "BUILD_VERSION is: <timestamp>"

# 3. Verify version in container
podman run --rm atreyu/naamnummer.web:latest cat /usr/share/nginx/html/index.html | grep "build-version"

# 4. Should see something like:
# <meta name="build-version" content="1704067200" />
```

---

## Additional Resources

- [Dockerfile Reference](https://docs.docker.com/engine/reference/builder/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Podman vs Docker](https://docs.podman.io/en/latest/)
- [hadolint - Dockerfile Linter](https://github.com/hadolint/hadolint)

---

**Status**: ✅ Fixed  
**Error**: Docker ENV doesn't support command substitution  
**Solution**: Use RUN command with shell instead  
**Applies to**: Docker, Podman, Buildah
