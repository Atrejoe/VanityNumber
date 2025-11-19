# Sentry Configuration Helper Script (PowerShell)
# This script helps you configure Sentry DSNs for both front-end and backend

param(
    [string]$WebDsn = "",
    [string]$ApiDsn = "",
    [switch]$Production,
    [switch]$Help
)

# Colors
$ColorReset = "`e[0m"
$ColorGreen = "`e[32m"
$ColorYellow = "`e[33m"
$ColorRed = "`e[31m"

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = $ColorReset
    )
    Write-Host "${Color}${Message}${ColorReset}"
}

function Show-Help {
    Write-Host ""
    Write-Host "Sentry Configuration Helper" -ForegroundColor Cyan
    Write-Host "=============================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Yellow
    Write-Host "  .\configure-sentry.ps1 [options]"
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  -WebDsn <dsn>      Front-end Sentry DSN"
    Write-Host "  -ApiDsn <dsn>      Backend API Sentry DSN"
    Write-Host "  -Production        Configure for Production (default: Development)"
    Write-Host "  -Help              Show this help message"
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  # Interactive mode"
    Write-Host "  .\configure-sentry.ps1"
    Write-Host ""
    Write-Host "  # Configure with DSNs"
    Write-Host "  .\configure-sentry.ps1 -WebDsn 'https://...' -ApiDsn 'https://...'"
    Write-Host ""
    Write-Host "  # Configure for Production"
    Write-Host "  .\configure-sentry.ps1 -Production -WebDsn 'https://...'"
    Write-Host ""
    exit 0
}

if ($Help) {
    Show-Help
}

function Update-JsonConfig {
    param(
        [string]$FilePath,
        [string]$Dsn,
        [string]$Environment,
        [double]$SampleRate
    )
    
    if (-not (Test-Path $FilePath)) {
        Write-ColorOutput "Error: File not found: $FilePath" $ColorRed
        return $false
    }
    
    # Create backup
    $backupPath = "$FilePath.backup"
    Copy-Item $FilePath $backupPath -Force
    
    # Read JSON
    $json = Get-Content $FilePath -Raw | ConvertFrom-Json
    
    # Update Sentry settings
    if (-not $json.Sentry) {
        $json | Add-Member -MemberType NoteProperty -Name "Sentry" -Value @{
            Dsn = ""
            Environment = "Development"
            TracesSampleRate = 1.0
            Debug = $false
        }
    }
    
    $json.Sentry.Dsn = $Dsn
    $json.Sentry.Environment = $Environment
    $json.Sentry.TracesSampleRate = $SampleRate
    
    # Write JSON
    $json | ConvertTo-Json -Depth 10 | Set-Content $FilePath
    
    Write-ColorOutput "  ✅ Updated: $FilePath" $ColorGreen
    return $true
}

# Main script
Write-Host ""
Write-ColorOutput "==================================================" $ColorGreen
Write-ColorOutput "  Vanity Number - Sentry Configuration Helper" $ColorGreen
Write-ColorOutput "==================================================" $ColorGreen
Write-Host ""

# Determine environment
if ($Production) {
    $environment = "Production"
    $sampleRate = 0.1
    $webConfig = "VanityNumber.Web\wwwroot\appsettings.Production.json"
    $apiConfig = "VanityNumber.Api\appsettings.Production.json"
} else {
    $environment = "Development"
    $sampleRate = 1.0
    $webConfig = "VanityNumber.Web\wwwroot\appsettings.json"
    $apiConfig = "VanityNumber.Api\appsettings.json"
}

Write-ColorOutput "Environment: $environment" $ColorYellow
Write-ColorOutput "Sample Rate: $sampleRate" $ColorYellow
Write-Host ""

# Configure Front-End
Write-ColorOutput "==================================================" $ColorGreen
Write-ColorOutput "  Step 1: Front-End Configuration (Blazor WASM)" $ColorGreen
Write-ColorOutput "==================================================" $ColorGreen
Write-Host ""

if ([string]::IsNullOrEmpty($WebDsn)) {
    Write-ColorOutput "Enter Sentry DSN for Front-End (Blazor):" $ColorYellow
    Write-Host "  (Leave empty to disable Sentry for front-end)"
    Write-Host "  Example: https://abc123@o123456.ingest.sentry.io/7654321"
    $WebDsn = Read-Host "DSN"
}

if ([string]::IsNullOrEmpty($WebDsn)) {
    Write-ColorOutput "  ⚠️  Sentry will be DISABLED for Front-End" $ColorYellow
} else {
    Write-ColorOutput "  ✅ Sentry will be enabled for Front-End" $ColorGreen
}

Write-Host ""
Write-Host "Updating Front-End configuration..."
Update-JsonConfig -FilePath $webConfig -Dsn $WebDsn -Environment $environment -SampleRate $sampleRate

# Configure Backend
Write-Host ""
Write-ColorOutput "==================================================" $ColorGreen
Write-ColorOutput "  Step 2: Backend Configuration (API)" $ColorGreen
Write-ColorOutput "==================================================" $ColorGreen
Write-Host ""

if ([string]::IsNullOrEmpty($ApiDsn)) {
    Write-ColorOutput "Enter Sentry DSN for Backend (API):" $ColorYellow
    Write-Host "  (Leave empty to disable Sentry for backend)"
    Write-Host "  Example: https://xyz789@o123456.ingest.sentry.io/7654322"
    $ApiDsn = Read-Host "DSN"
}

if ([string]::IsNullOrEmpty($ApiDsn)) {
    Write-ColorOutput "  ⚠️  Sentry will be DISABLED for Backend" $ColorYellow
} else {
    Write-ColorOutput "  ✅ Sentry will be enabled for Backend" $ColorGreen
}

Write-Host ""
Write-Host "Updating Backend configuration..."
Update-JsonConfig -FilePath $apiConfig -Dsn $ApiDsn -Environment $environment -SampleRate $sampleRate

# Summary
Write-Host ""
Write-ColorOutput "==================================================" $ColorGreen
Write-ColorOutput "  Configuration Summary" $ColorGreen
Write-ColorOutput "==================================================" $ColorGreen
Write-Host ""
Write-ColorOutput "Front-End (Blazor WASM):" $ColorYellow
Write-Host "  File: $webConfig"
Write-Host "  DSN: $(if ($WebDsn) { $WebDsn } else { '[DISABLED]' })"
Write-Host "  Environment: $environment"
Write-Host "  Sample Rate: $sampleRate"
Write-Host ""
Write-ColorOutput "Backend (API):" $ColorYellow
Write-Host "  File: $apiConfig"
Write-Host "  DSN: $(if ($ApiDsn) { $ApiDsn } else { '[DISABLED]' })"
Write-Host "  Environment: $environment"
Write-Host "  Sample Rate: $sampleRate"
Write-Host ""

Write-ColorOutput "==================================================" $ColorGreen
Write-ColorOutput "  Next Steps" $ColorGreen
Write-ColorOutput "==================================================" $ColorGreen
Write-Host ""
Write-Host "1. Review the updated configuration files"
Write-Host "2. Build your project: dotnet build"
Write-Host "3. Test Sentry integration:"
Write-Host "   - Front-End: Add error button to Home.razor"
Write-Host "   - Backend: Call /api/test-sentry endpoint"
Write-Host "4. Check Sentry dashboard for events"
Write-Host ""
Write-Host "Backup files created:"
Write-Host "  - $webConfig.backup"
Write-Host "  - $apiConfig.backup"
Write-Host ""
Write-ColorOutput "✅ Configuration complete!" $ColorGreen
Write-Host ""
Write-Host "For more information, see:"
Write-Host "  - SENTRY_QUICK_START.md"
Write-Host "  - SENTRY_MONITORING_GUIDE.md"
Write-Host ""
