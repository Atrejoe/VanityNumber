# Use the official .NET 10 RC SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:11.0-preview AS build
WORKDIR /src

# Install required dependencies for wasm-tools workload
RUN apt-get update && \
    apt-get install -y python3 python3-pip clang build-essential && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Install wasm-tools workload for optimized Blazor WebAssembly builds
RUN dotnet workload install wasm-tools

# Copy solution file
COPY VanityNumber.sln ./

# Copy project files for restore (improves Docker layer caching)
COPY VanityNumber.Web/VanityNumber.Web.csproj VanityNumber.Web/
COPY VanityNumber.Contracts/VanityNumber.Contracts.csproj VanityNumber.Contracts/

# Restore dependencies
RUN dotnet restore VanityNumber.Web/VanityNumber.Web.csproj

# Copy all source files (including scripts)
COPY VanityNumber.Web/ VanityNumber.Web/
COPY VanityNumber.Contracts/ VanityNumber.Contracts/

# Build and publish the Blazor WebAssembly application
WORKDIR /src/VanityNumber.Web
RUN dotnet publish VanityNumber.Web.csproj -c Release -o /app/publish

# Debug: List files to see what we have
RUN echo "=== Files in current directory ===" && ls -la

# Apply cache busting with build timestamp
ARG BUILD_VERSION

# Apply cache busting - use inline script since file might not be there
RUN BUILD_VERSION="${BUILD_VERSION:-$(date +%s)}" && \
    echo "🔄 Applying cache busting with version: $BUILD_VERSION" && \
    WWW_ROOT="/app/publish/wwwroot" && \
    if [ -f "$WWW_ROOT/index.html" ]; then \
        echo "  ✓ Updating index.html"; \
        sed -i "s/{{BUILD_VERSION}}/$BUILD_VERSION/g" "$WWW_ROOT/index.html"; \
    else \
        echo "  ⚠ Warning: index.html not found"; \
    fi && \
    if [ -f "$WWW_ROOT/manifest.json" ]; then \
        echo "  ✓ Updating manifest.json"; \
        if grep -q '"version"' "$WWW_ROOT/manifest.json"; then \
            sed -i "s/\"version\": \"[^\"]*\"/\"version\": \"$BUILD_VERSION\"/g" "$WWW_ROOT/manifest.json"; \
        else \
            sed -i "s/\"name\": \"/\"version\": \"$BUILD_VERSION\",\n  \"name\": \"/g" "$WWW_ROOT/manifest.json"; \
        fi; \
    fi && \
    if [ -f "$WWW_ROOT/service-worker.js" ]; then \
        echo "  ✓ Updating service-worker.js"; \
        sed -i "s/const CACHE_NAME = 'vanity-number-v[^']*'/const CACHE_NAME = 'vanity-number-v$BUILD_VERSION'/g" "$WWW_ROOT/service-worker.js"; \
        sed -i "s/const RUNTIME_CACHE = 'vanity-number-runtime'/const RUNTIME_CACHE = 'vanity-number-runtime-v$BUILD_VERSION'/g" "$WWW_ROOT/service-worker.js"; \
    fi && \
    echo "✅ Cache busting applied successfully" && \
    echo "📦 Build Version: $BUILD_VERSION"

# Verify publish output
RUN echo "=== Published files ===" && \
    ls -lah /app/publish/wwwroot && \
    echo "=== Framework files ===" && \
    ls -lah /app/publish/wwwroot/_framework/ || echo "No _framework directory" && \
    echo "=== Build Version Check ===" && \
    grep -o 'build-version" content="[^"]*"' /app/publish/wwwroot/index.html || echo "⚠️  No build version found - cache busting may not have worked"

# Build runtime image using nginx to serve static files
FROM nginx:alpine AS final

# Set working directory
WORKDIR /usr/share/nginx/html

# Remove default nginx static content
RUN rm -rf ./*

# Copy published Blazor WebAssembly files
COPY --from=build /app/publish/wwwroot .

# Verify files were copied
RUN echo "=== Files in nginx html root ===" && \
    ls -lah /usr/share/nginx/html && \
    echo "=== Framework files ===" && \
    ls -lah /usr/share/nginx/html/_framework/ || echo "No _framework directory" && \
    echo "=== Checking cache busting ===" && \
    grep -o 'build-version" content="[^"]*"' /usr/share/nginx/html/index.html || echo "No build version found"

# Copy startup script to update configuration at runtime
COPY VanityNumber.Web/update-config.sh /usr/local/bin/update-config.sh
RUN chmod +x /usr/local/bin/update-config.sh

# Copy custom nginx configuration (already configured for port 8080)
COPY VanityNumber.Web/nginx.conf /etc/nginx/nginx.conf

# Create necessary directories and set permissions for non-root user
RUN mkdir -p /var/cache/nginx /var/log/nginx /var/run && \
    chown -R nginx:nginx /usr/share/nginx/html /var/cache/nginx /var/log/nginx /var/run /etc/nginx && \
    chmod -R 755 /usr/share/nginx/html && \
    touch /var/run/nginx.pid && \
    chown nginx:nginx /var/run/nginx.pid

# Expose port 8080 (non-privileged port)
EXPOSE 8080

# Switch to non-root user
USER nginx

# Start nginx with configuration update
CMD ["/usr/local/bin/update-config.sh"]
