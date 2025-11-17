# Quick-Build.ps1
# Quick build script for daily development

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("build", "test", "deploy", "full")]
    [string]$Action = "build",

    [Parameter(Mandatory=$false)]
    [string]$Tag = "latest"
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "🚀 Vanity Number - Quick Build" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

switch ($Action) {
    "build" {
        Write-Host "Building image..." -ForegroundColor Yellow
        .\Build-VanityNumberWeb.ps1 -ImageTag $Tag
    }

    "test" {
        Write-Host "Building and testing image..." -ForegroundColor Yellow
        .\Build-VanityNumberWeb.ps1 -ImageTag $Tag -Test
    }

    "deploy" {
        Write-Host "Building, testing, and pushing to registry..." -ForegroundColor Yellow
        .\Build-VanityNumberWeb.ps1 -ImageTag $Tag -Test -Push

        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "Deploying to Kubernetes..." -ForegroundColor Yellow
            kubectl rollout restart deployment/vanity-robertsirre-nl -n vanity-robertsirre-nl

            Write-Host ""
            Write-Host "Waiting for rollout..." -ForegroundColor Yellow
            kubectl rollout status deployment/vanity-robertsirre-nl -n vanity-robertsirre-nl

            Write-Host ""
            Write-Host "✅ Deployment complete!" -ForegroundColor Green
            Write-Host "🌐 Visit: https://vanity.robertsirre.nl" -ForegroundColor Cyan
        }
    }

    "full" {
        Write-Host "Full rebuild (no cache)..." -ForegroundColor Yellow
        .\Build-VanityNumberWeb.ps1 -ImageTag $Tag -NoBuildCache -Test -Push

        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "Deploying to Kubernetes..." -ForegroundColor Yellow
            kubectl set image deployment/vanity-robertsirre-nl `
                api=atreyu/vanitynumber.web:$Tag `
                -n vanity-robertsirre-nl

            kubectl rollout status deployment/vanity-robertsirre-nl -n vanity-robertsirre-nl

            Write-Host ""
            Write-Host "✅ Full rebuild and deployment complete!" -ForegroundColor Green
        }
    }
}

Write-Host ""
