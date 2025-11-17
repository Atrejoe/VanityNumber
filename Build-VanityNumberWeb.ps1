# Build-VanityNumberWeb.ps1
# Advanced build script with multiple options for Podman

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$ImageTag = "latest",
    
    [Parameter(Mandatory=$false)]
    [string]$Registry = "atreyu",
    
    [Parameter(Mandatory=$false)]
    [switch]$Push,
    
    [Parameter(Mandatory=$false)]
    [switch]$Test,
    
    [Parameter(Mandatory=$false)]
    [switch]$NoBuildCache,
    
    [Parameter(Mandatory=$false)]
    [string]$BuildVersion
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Generate build version if not provided
if ([string]::IsNullOrEmpty($BuildVersion)) {
    $BuildVersion = [int][double]::Parse((Get-Date -UFormat %s))
}

$ImageName = "$Registry/vanitynumber.web"
$DockerfilePath = ".\VanityNumber.Web\Dockerfile"

# Display banner
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Vanity Number Web - Container Build Script (Podman)      ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Display build information
Write-Host "📋 Build Configuration:" -ForegroundColor Yellow
Write-Host "   Build Version:  $BuildVersion" -ForegroundColor White
Write-Host "   Image Name:     $ImageName" -ForegroundColor White
Write-Host "   Image Tag:      $ImageTag" -ForegroundColor White
Write-Host "   Dockerfile:     $DockerfilePath" -ForegroundColor White
Write-Host "   No Cache:       $NoBuildCache" -ForegroundColor White
Write-Host "   Auto Push:      $Push" -ForegroundColor White
Write-Host "   Auto Test:      $Test" -ForegroundColor White
Write-Host ""

# Verify Dockerfile exists
if (-not (Test-Path $DockerfilePath)) {
    Write-Host "❌ Dockerfile not found at: $DockerfilePath" -ForegroundColor Red
    exit 1
}

# Verify Podman is installed
try {
    $podmanVersion = podman --version
    Write-Host "✓ Podman detected: $podmanVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Podman is not installed or not in PATH" -ForegroundColor Red
    Write-Host "   Install from: https://podman.io/getting-started/installation" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Build the image
Write-Host "🏗️  Building container image..." -ForegroundColor Cyan
Write-Host ""

$buildArgs = @(
    "build"
    "--build-arg", "BUILD_VERSION=$BuildVersion"
    "-t", "$ImageName`:$ImageTag"
    "-t", "$ImageName`:v$BuildVersion"
    "-f", $DockerfilePath
)

if ($NoBuildCache) {
    $buildArgs += "--no-cache"
}

$buildArgs += "."

try {
    Write-Host "Executing: podman $($buildArgs -join ' ')" -ForegroundColor Gray
    Write-Host ""
    
    & podman $buildArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "Podman build failed with exit code $LASTEXITCODE"
    }
    
    Write-Host ""
    Write-Host "✅ Build completed successfully!" -ForegroundColor Green
    Write-Host ""
    
} catch {
    Write-Host ""
    Write-Host "❌ Build failed: $_" -ForegroundColor Red
    exit 1
}

# Display created images
Write-Host "📦 Images created:" -ForegroundColor Cyan
try {
    podman images $ImageName --format "table {{.Repository}}:{{.Tag}}`t{{.Size}}`t{{.Created}}" | Select-Object -First 3
} catch {
    Write-Host "   - $ImageName`:$ImageTag" -ForegroundColor White
    Write-Host "   - $ImageName`:v$BuildVersion" -ForegroundColor White
}
Write-Host ""

# Test the image if requested
if ($Test) {
    Write-Host "🧪 Testing container..." -ForegroundColor Cyan
    Write-Host ""
    
    $containerName = "vanity-test-$BuildVersion"
    
    try {
        Write-Host "Starting test container: $containerName" -ForegroundColor Gray
        podman run -d --name $containerName -p 8080:8080 "$ImageName`:$ImageTag" | Out-Null
        
        Start-Sleep -Seconds 3
        
        Write-Host "Testing HTTP endpoint..." -ForegroundColor Gray
        $response = Invoke-WebRequest -Uri "http://localhost:8080/health" -TimeoutSec 10 -UseBasicParsing
        
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ Container is running and healthy!" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Container returned status code: $($response.StatusCode)" -ForegroundColor Yellow
        }
        
    } catch {
        Write-Host "❌ Test failed: $_" -ForegroundColor Red
    } finally {
        Write-Host "Stopping and removing test container..." -ForegroundColor Gray
        podman stop $containerName 2>$null | Out-Null
        podman rm $containerName 2>$null | Out-Null
    }
    
    Write-Host ""
}

# Push to registry if requested
if ($Push) {
    Write-Host "🚀 Pushing images to registry..." -ForegroundColor Cyan
    Write-Host ""
    
    try {
        Write-Host "Pushing $ImageName`:$ImageTag..." -ForegroundColor Gray
        podman push "$ImageName`:$ImageTag"
        
        Write-Host "Pushing $ImageName`:v$BuildVersion..." -ForegroundColor Gray
        podman push "$ImageName`:v$BuildVersion"
        
        Write-Host ""
        Write-Host "✅ Images pushed successfully!" -ForegroundColor Green
        Write-Host ""
        
    } catch {
        Write-Host ""
        Write-Host "❌ Push failed: $_" -ForegroundColor Red
        Write-Host "   Make sure you're logged in: podman login" -ForegroundColor Yellow
        exit 1
    }
}

# Display next steps
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "✨ Next Steps:" -ForegroundColor Yellow
Write-Host ""

if (-not $Test) {
    Write-Host "🧪 Test locally:" -ForegroundColor Cyan
    Write-Host "   podman run -p 8080:8080 $ImageName`:$ImageTag" -ForegroundColor White
    Write-Host "   Then visit: http://localhost:8080" -ForegroundColor Gray
    Write-Host ""
}

if (-not $Push) {
    Write-Host "🚀 Push to registry:" -ForegroundColor Cyan
    Write-Host "   podman push $ImageName`:$ImageTag" -ForegroundColor White
    Write-Host "   podman push $ImageName`:v$BuildVersion" -ForegroundColor White
    Write-Host ""
}

Write-Host "📦 Deploy to Kubernetes:" -ForegroundColor Cyan
Write-Host "   kubectl set image deployment/vanity-robertsirre-nl ``" -ForegroundColor White
Write-Host "       api=$ImageName`:$ImageTag ``" -ForegroundColor White
Write-Host "       -n vanity-robertsirre-nl" -ForegroundColor White
Write-Host ""
Write-Host "   # Or rollout restart:" -ForegroundColor Gray
Write-Host "   kubectl rollout restart deployment/vanity-robertsirre-nl ``" -ForegroundColor White
Write-Host "       -n vanity-robertsirre-nl" -ForegroundColor White
Write-Host ""

Write-Host "🔍 Verify deployment:" -ForegroundColor Cyan
Write-Host "   kubectl logs -n vanity-robertsirre-nl -l app=vanity-robertsirre-nl --tail=50" -ForegroundColor White
Write-Host ""

Write-Host "📋 Build Version: $BuildVersion" -ForegroundColor Yellow
Write-Host "   Use this for tracking in Kubernetes" -ForegroundColor Gray
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
