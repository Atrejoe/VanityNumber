# build-with-version.ps1
# Build container image with timestamp-based cache busting using Podman

param(
    [Parameter(Mandatory=$false)]
    [string]$ImageTag = "latest"
)

# Generate build version (Unix timestamp)
$BuildVersion = [int][double]::Parse((Get-Date -UFormat %s))
$ImageName = "atreyu/vanitynumber.web"

Write-Host "🏗️  Building Vanity Number Web with cache busting" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Build Version: $BuildVersion" -ForegroundColor Yellow
Write-Host "Image: $ImageName`:$ImageTag" -ForegroundColor Yellow
Write-Host ""

# Build with Podman
try {
    Write-Host "Building container image..." -ForegroundColor Green

    podman build `
        --build-arg BUILD_VERSION="$BuildVersion" `
        -t "$ImageName`:$ImageTag" `
        -t "$ImageName`:v$BuildVersion" `
        -f .\VanityNumber.Web\Dockerfile `
        .

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }

    Write-Host ""
    Write-Host "✅ Build complete!" -ForegroundColor Green
    Write-Host "📦 Images created:" -ForegroundColor Cyan
    Write-Host "   - $ImageName`:$ImageTag" -ForegroundColor White
    Write-Host "   - $ImageName`:v$BuildVersion" -ForegroundColor White
    Write-Host ""
    Write-Host "🧪 Test locally:" -ForegroundColor Cyan
    Write-Host "   podman run -p 8080:8080 $ImageName`:$ImageTag" -ForegroundColor White
    Write-Host ""
    Write-Host "🚀 Push to registry:" -ForegroundColor Cyan
    Write-Host "   podman push $ImageName`:$ImageTag" -ForegroundColor White
    Write-Host "   podman push $ImageName`:v$BuildVersion" -ForegroundColor White
    Write-Host ""
    Write-Host "📋 Build Version: $BuildVersion" -ForegroundColor Yellow
    Write-Host "   Use this version for Kubernetes deployment" -ForegroundColor Gray

} catch {
    Write-Host "❌ Error during build: $_" -ForegroundColor Red
    exit 1
}
