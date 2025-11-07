# Download and Sanitize Dictionaries for Vanity Number API v2
# This script downloads fresh dictionaries and sanitizes them for phone number conversion
# NEW: Preserves original spelling with diacritics for display
# Format: NORMALIZED<tab>ORIGINAL (e.g., "CAFE	café")
# Maximum phone number length: 10 digits
# Phone keypad mapping: 2=ABC, 3=DEF, 4=GHI, 5=JKL, 6=MNO, 7=PQRS, 8=TUV, 9=WXYZ

Write-Host "=== Downloading and Sanitizing Dictionaries v2 ===" -ForegroundColor Green
Write-Host ""

$dictionariesPath = $PSScriptRoot

# Function to remove diacritics and normalize to A-Z
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

# Function to check if a word is an abbreviation (all caps, 2-5 chars)
function Is-Abbreviation {
    param([string]$word)
    
    if ($word.Length -ge 2 -and $word.Length -le 5) {
        # Check if all letters are uppercase
        if ($word -ceq $word.ToUpper() -and $word -match '^[A-Z]+$') {
            return $true
        }
    }
    return $false
}

$validPhoneLetters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'

# Function to sanitize dictionary - NEW FORMAT
function Sanitize-Dictionary {
    param(
        [string]$InputFile,
        [string]$OutputFile,
        [int]$MinLength = 3,
        [int]$MaxLength = 10,
        [string]$Language
    )
    
    Write-Host "Sanitizing $Language dictionary..." -ForegroundColor Yellow
    Write-Host "  Reading file..." -ForegroundColor Gray
    
    $words = Get-Content $InputFile -ErrorAction Stop
    $originalCount = $words.Count
    Write-Host "  Processing $originalCount words..." -ForegroundColor Gray
    
    # Use hashtable to track: normalized -> list of original forms
    $wordMap = @{}
    $counter = 0
    
    foreach ($word in $words) {
        $counter++
        if ($counter % 50000 -eq 0) {
            Write-Host "    Processed $counter words..." -ForegroundColor DarkGray
        }
        
        if ([string]::IsNullOrWhiteSpace($word)) {
            continue
        }
        
        $trimmed = $word.Trim()
        
        # Check if abbreviation (preserve as-is)
        $isAbbr = Is-Abbreviation $trimmed
        
        # Normalize for matching
        $normalized = Remove-Diacritics $trimmed
        
        # Only keep words with valid letters
        if ($normalized -match '^[a-zA-Z]+$') {
            $normalizedUpper = $normalized.ToUpper()
            
            # Check length
            if ($normalizedUpper.Length -ge $MinLength -and $normalizedUpper.Length -le $MaxLength) {
                # Verify all letters can be mapped to phone digits
                $allValid = $true
                foreach ($char in $normalizedUpper.ToCharArray()) {
                    if ($validPhoneLetters -notcontains $char) {
                        $allValid = $false
                        break
                    }
                }
                
                if ($allValid) {
                    # Store original form (preserve case and diacritics)
                    if (-not $wordMap.ContainsKey($normalizedUpper)) {
                        $wordMap[$normalizedUpper] = @()
                    }
                    
                    # Add original if not already present (case-sensitive check)
                    $original = if ($isAbbr) { $trimmed } else { $trimmed }
                    if ($wordMap[$normalizedUpper] -notcontains $original) {
                        $wordMap[$normalizedUpper] += $original
                    }
                }
            }
        }
    }
    
    Write-Host "  Building output (normalized<tab>original format)..." -ForegroundColor Gray
    
    # Create output lines: NORMALIZED<tab>ORIGINAL
    $outputLines = @()
    foreach ($normalized in ($wordMap.Keys | Sort-Object)) {
        $originals = $wordMap[$normalized]
        
        # Prefer original with diacritics, then mixed case, then abbreviations
        $preferred = $originals | Where-Object { $_ -match '[àâäæçéèêëïîôùûüÿœ]' } | Select-Object -First 1
        if (-not $preferred) {
            $preferred = $originals | Where-Object { Is-Abbreviation $_ } | Select-Object -First 1
        }
        if (-not $preferred) {
            $preferred = $originals | Where-Object { $_ -cne $_.ToUpper() } | Select-Object -First 1
        }
        if (-not $preferred) {
            $preferred = $originals[0]
        }
        
        # Output format: NORMALIZED<tab>ORIGINAL
        $outputLines += "$normalized`t$preferred"
    }
    
    $sanitizedCount = $outputLines.Count
    
    # Write to output file
    Write-Host "  Writing to file..." -ForegroundColor Gray
    $outputLines | Set-Content $OutputFile -Encoding UTF8
    
    Write-Host "  Original: $originalCount words" -ForegroundColor Gray
    Write-Host "  Sanitized: $sanitizedCount unique normalized forms" -ForegroundColor Gray
    Write-Host "  Format: NORMALIZED<tab>ORIGINAL (preserves diacritics)" -ForegroundColor Gray
    Write-Host ""
}

# Download Dutch dictionary
Write-Host "Downloading Dutch dictionary from OpenTaal..." -ForegroundColor Cyan
$dutchUrl = "https://raw.githubusercontent.com/OpenTaal/opentaal-wordlist/master/wordlist.txt"
$dutchRaw = Join-Path $dictionariesPath "dutch_raw.txt"
$ProgressPreference = 'SilentlyContinue'

if (-not (Test-Path $dutchRaw)) {
    Invoke-WebRequest -Uri $dutchUrl -OutFile $dutchRaw
    Write-Host "  Downloaded to: $dutchRaw" -ForegroundColor Gray
} else {
    Write-Host "  Using existing: $dutchRaw" -ForegroundColor Gray
}
Write-Host ""

# Download English dictionary
Write-Host "Downloading English dictionary from dwyl/english-words..." -ForegroundColor Cyan
$englishUrl = "https://raw.githubusercontent.com/dwyl/english-words/master/words_alpha.txt"
$englishRaw = Join-Path $dictionariesPath "english_raw.txt"

if (-not (Test-Path $englishRaw)) {
    Invoke-WebRequest -Uri $englishUrl -OutFile $englishRaw
    Write-Host "  Downloaded to: $englishRaw" -ForegroundColor Gray
} else {
    Write-Host "  Using existing: $englishRaw" -ForegroundColor Gray
}
Write-Host ""

# Sanitize Dutch dictionary
Sanitize-Dictionary -InputFile $dutchRaw -OutputFile (Join-Path $dictionariesPath "dutch.txt") -Language "Dutch"

# Sanitize English dictionary
Sanitize-Dictionary -InputFile $englishRaw -OutputFile (Join-Path $dictionariesPath "english.txt") -Language "English"

# Urban dictionary - sanitize existing
Write-Host "Sanitizing Urban dictionary..." -ForegroundColor Yellow
$urbanFile = Join-Path $dictionariesPath "urban.txt"
if (Test-Path $urbanFile) {
    $urbanBackup = Join-Path $dictionariesPath "urban_backup.txt"
    Copy-Item $urbanFile $urbanBackup
    
    Sanitize-Dictionary -InputFile $urbanBackup -OutputFile $urbanFile -Language "Urban"
} else {
    Write-Host "  Urban dictionary not found, skipping" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "=== Dictionary Sanitization Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Final word counts:" -ForegroundColor Yellow

$dutchFinal = (Get-Content (Join-Path $dictionariesPath "dutch.txt")).Count
Write-Host "  Dutch: $dutchFinal words" -ForegroundColor White

$englishFinal = (Get-Content (Join-Path $dictionariesPath "english.txt")).Count
Write-Host "  English: $englishFinal words" -ForegroundColor White

if (Test-Path $urbanFile) {
    $urbanFinal = (Get-Content $urbanFile).Count
    Write-Host "  Urban: $urbanFinal words" -ForegroundColor White
}

Write-Host ""
Write-Host "Format: Each line contains NORMALIZED<tab>ORIGINAL" -ForegroundColor Gray
Write-Host "Example: CAFE	café (preserves diacritics for display)" -ForegroundColor Gray
Write-Host ""

# Show sample entries
Write-Host "Sample entries from each dictionary:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Dutch (first 5 with diacritics):" -ForegroundColor Cyan
Get-Content (Join-Path $dictionariesPath "dutch.txt") | Where-Object { $_ -match '\t.*[àâäæçéèêëïîôùûüÿœ]' } | Select-Object -First 5 | ForEach-Object {
    Write-Host "  $_" -ForegroundColor Gray
}

Write-Host ""
Write-Host "All dictionaries ready for use!" -ForegroundColor Green
