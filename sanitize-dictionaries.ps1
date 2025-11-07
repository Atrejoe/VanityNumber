# Sanitize all dictionaries to only include valid words for phone number conversion
# Valid words: Only A-Z letters, length 1-10 characters

$dictionaryPath = "VanityNumberApi.Core\Dictionaries"

function Sanitize-Dictionary {
    param(
        [string]$filename
    )
    
    $filePath = Join-Path $dictionaryPath $filename
    Write-Host "Processing $filename..." -ForegroundColor Yellow
    
    $originalContent = Get-Content $filePath
    $originalCount = $originalContent.Count
    
    # Filter: only A-Z, length 1-10, remove duplicates
    $sanitized = $originalContent | 
        Where-Object { $_ -match '^[A-Z]{1,10}$' } | 
        Sort-Object -Unique
    
    $newCount = $sanitized.Count
    $removed = $originalCount - $newCount
    
    # Backup original
    $backupPath = "$filePath.backup"
    Copy-Item $filePath $backupPath -Force
    
    # Write sanitized content
    $sanitized | Set-Content $filePath -Encoding UTF8
    
    Write-Host "  Original: $originalCount words" -ForegroundColor Gray
    Write-Host "  Sanitized: $newCount words" -ForegroundColor Green
    Write-Host "  Removed: $removed words" -ForegroundColor Red
    Write-Host "  Backup saved to: $backupPath" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "=== Dictionary Sanitization ===" -ForegroundColor Green
Write-Host "Sanitizing dictionaries to contain only valid words (A-Z, 1-10 characters)" -ForegroundColor White
Write-Host ""

Sanitize-Dictionary "dutch.txt"
Sanitize-Dictionary "english.txt"
Sanitize-Dictionary "urban.txt"

Write-Host "=== Sanitization Complete ===" -ForegroundColor Green
