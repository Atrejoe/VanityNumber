#!/bin/sh
# apply-cache-busting.sh
# Adds build timestamp to static resources for cache busting

set -e

WWW_ROOT="/app/publish/wwwroot"
BUILD_VERSION="${BUILD_VERSION:-$(date +%s)}"

echo "🔄 Applying cache busting with version: $BUILD_VERSION"

# Replace {{BUILD_VERSION}} placeholder in index.html
if [ -f "$WWW_ROOT/index.html" ]; then
    echo "  ✓ Updating index.html"
    sed -i "s/{{BUILD_VERSION}}/$BUILD_VERSION/g" "$WWW_ROOT/index.html"
else
    echo "  ⚠ Warning: index.html not found"
fi

# Update manifest.json with version
if [ -f "$WWW_ROOT/manifest.json" ]; then
    echo "  ✓ Updating manifest.json"
    # Add version field to manifest
    if grep -q '"version"' "$WWW_ROOT/manifest.json"; then
        sed -i "s/\"version\": \"[^\"]*\"/\"version\": \"$BUILD_VERSION\"/g" "$WWW_ROOT/manifest.json"
    else
        # Add version field after name
        sed -i "s/\"name\": \"/\"version\": \"$BUILD_VERSION\",\n  \"name\": \"/g" "$WWW_ROOT/manifest.json"
    fi
fi

# Update service worker cache version
if [ -f "$WWW_ROOT/service-worker.js" ]; then
    echo "  ✓ Updating service-worker.js"
    sed -i "s/const CACHE_NAME = 'vanity-number-v[^']*'/const CACHE_NAME = 'vanity-number-v$BUILD_VERSION'/g" "$WWW_ROOT/service-worker.js"
    sed -i "s/const RUNTIME_CACHE = 'vanity-number-runtime'/const RUNTIME_CACHE = 'vanity-number-runtime-v$BUILD_VERSION'/g" "$WWW_ROOT/service-worker.js"
fi

echo "✅ Cache busting applied successfully"
echo "📦 Build Version: $BUILD_VERSION"
