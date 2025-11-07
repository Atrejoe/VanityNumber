# Sanitize existing raw dictionaries
# Keeps simple format: one word per line (normalized, uppercase)

Write-Host "=== Sanitizing Dictionaries ===" -ForegroundColor Green

$dictionariesPath = $PSScriptRoot

function Remove-Diacritics {
    param([string]$text)
    
    $normalized = $text.Normalize([Text.NormalizationForm]::FormD)
    $stringBuilder = New-Object Text.StringBuilder
    
    foreach ($c in $normalized.ToCharArray()) {
        $unicodeCategory = [Globalization.CharUnicodeInfo]::GetUnicodeCategory($c)
        if ($unicodeCategory -ne [Globalization.UnicodeCategory]::NonSpacingMark) {
            [void]$stringBuilder.Append($c)
        }
    }
    
    return $stringBuilder.ToString().Normalize([Text.NormalizationForm]::FormC)
}

function Sanitize-Dict {
    param(
        [string]$InputFile,
        [string]$OutputFile,
        [string]$Name
    )
    
    Write-Host "Sanitizing $Name..." -ForegroundColor Yellow
    
    $words = Get-Content $InputFile
    Write-Host "  Input: $($words.Count) words" -ForegroundColor Gray
    
    $sanitized = $words |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        ForEach-Object { Remove-Diacritics $_.Trim() } |
        Where-Object { $_ -match '^[a-zA-Z]+$' } |
        Where-Object { $_.Length -ge 3 -and $_.Length -le 10 } |
        ForEach-Object { $_.ToUpper() } |
        Sort-Object -Unique
    
    $sanitized | Set-Content $OutputFile -Encoding UTF8
    
    Write-Host "  Output: $($sanitized.Count) words" -ForegroundColor Green
    Write-Host ""
}

# Sanitize each dictionary
Sanitize-Dict -InputFile (Join-Path $dictionariesPath "dutch_raw.txt") `
              -OutputFile (Join-Path $dictionariesPath "dutch.txt") `
              -Name "Dutch"

Sanitize-Dict -InputFile (Join-Path $dictionariesPath "english_raw.txt") `
              -OutputFile (Join-Path $dictionariesPath "english.txt") `
              -Name "English"

$urbanFile = Join-Path $dictionariesPath "urban.txt"
if (Test-Path $urbanFile) {
    $urbanBackup = Join-Path $dictionariesPath "urban_backup.txt"
    Copy-Item $urbanFile $urbanBackup -Force
    
    Sanitize-Dict -InputFile $urbanBackup `
                  -OutputFile $urbanFile `
                  -Name "Urban"
}

Write-Host "=== Complete ===" -ForegroundColor Green
Write-Host "Dictionaries sanitized: A-Z only, 3-10 characters, uppercase" -ForegroundColor Gray
