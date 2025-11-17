#!/bin/bash
# build-with-version.sh
# Build Docker image with timestamp-based cache busting

set -e

# Generate build version (Unix timestamp)
BUILD_VERSION=$(date +%s)
IMAGE_TAG="${1:-latest}"
IMAGE_NAME="atreyu/vanitynumber.web"

echo "🏗️  Building Vanity Number Web with cache busting"
echo "================================================"
echo "Build Version: $BUILD_VERSION"
echo "Image: $IMAGE_NAME:$IMAGE_TAG"
echo ""

# Build Docker image with build version
docker build \
    --build-arg BUILD_VERSION="$BUILD_VERSION" \
    -t "$IMAGE_NAME:$IMAGE_TAG" \
    -t "$IMAGE_NAME:v$BUILD_VERSION" \
    -f VanityNumber.Web/Dockerfile \
    .

echo ""
echo "✅ Build complete!"
echo "📦 Images created:"
echo "   - $IMAGE_NAME:$IMAGE_TAG"
echo "   - $IMAGE_NAME:v$BUILD_VERSION"
echo ""
echo "🧪 Test locally:"
echo "   docker run -p 8080:8080 $IMAGE_NAME:$IMAGE_TAG"
echo ""
echo "🚀 Push to registry:"
echo "   docker push $IMAGE_NAME:$IMAGE_TAG"
echo "   docker push $IMAGE_NAME:v$BUILD_VERSION"
echo ""
echo "📋 Build Version: $BUILD_VERSION"
echo "   Use this version for Kubernetes deployment"
