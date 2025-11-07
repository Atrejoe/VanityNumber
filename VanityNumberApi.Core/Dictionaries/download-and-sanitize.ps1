# Download and Sanitize Dictionaries for Vanity Number API
# This script downloads fresh dictionaries and sanitizes them for phone number conversion
# Maximum phone number length: 10 digits
# Phone keypad mapping: 2=ABC, 3=DEF, 4=GHI, 5=JKL, 6=MNO, 7=PQRS, 8=TUV, 9=WXYZ
# Only letters A-Z that can be mapped from phone keypad
# Diacritics are normalized (é→E, ü→U, etc.)

Write-Host "=== Downloading and Sanitizing Dictionaries ===" -ForegroundColor Green
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

# Phone keypad letters: A-Z excluding Q and Z variations
# 2=ABC, 3=DEF, 4=GHI, 5=JKL, 6=MNO, 7=PQRS, 8=TUV, 9=WXYZ
$validPhoneLetters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'

# Function to sanitize dictionary
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
    
    # Read all lines
    $words = Get-Content $InputFile -ErrorAction Stop
    $originalCount = $words.Count
    Write-Host "  Processing $originalCount words..." -ForegroundColor Gray
    
    $sanitized = @()
    $counter = 0
    
    foreach ($word in $words) {
        $counter++
        if ($counter % 50000 -eq 0) {
            Write-Host "    Processed $counter words..." -ForegroundColor DarkGray
        }
        
        # Skip if empty
        if ([string]::IsNullOrWhiteSpace($word)) {
            continue
        }
        
        # Normalize diacritics (é→e, ü→u, etc.)
        $normalized = Remove-Diacritics $word
        
        # Only keep words with valid phone keypad letters (A-Z)
        # No numbers, hyphens, apostrophes, or other special characters
        if ($normalized -match '^[a-zA-Z]+$') {
            $upper = $normalized.ToUpper()
            
            # Check length
            if ($upper.Length -ge $MinLength -and $upper.Length -le $MaxLength) {
                # Verify all letters can be mapped to phone digits
                $allValid = $true
                foreach ($char in $upper.ToCharArray()) {
                    if ($validPhoneLetters -notcontains $char) {
                        $allValid = $false
                        break
                    }
                }
                
                if ($allValid) {
                    $sanitized += $upper
                }
            }
        }
    }
    
    Write-Host "  Removing duplicates and sorting..." -ForegroundColor Gray
    $sanitized = $sanitized | Sort-Object -Unique
    
    $sanitizedCount = $sanitized.Count
    
    # Write to output file
    Write-Host "  Writing to file..." -ForegroundColor Gray
    $sanitized | Set-Content $OutputFile -Encoding UTF8
    
    Write-Host "  Original: $originalCount words" -ForegroundColor Gray
    Write-Host "  Sanitized: $sanitizedCount words (normalized diacritics)" -ForegroundColor Gray
    Write-Host "  Removed: $($originalCount - $sanitizedCount) words" -ForegroundColor Gray
    Write-Host ""
}

# Download Dutch dictionary
Write-Host "Downloading Dutch dictionary from OpenTaal..." -ForegroundColor Cyan
$dutchUrl = "https://raw.githubusercontent.com/OpenTaal/opentaal-wordlist/master/wordlist.txt"
$dutchRaw = Join-Path $dictionariesPath "dutch_raw.txt"
$ProgressPreference = 'SilentlyContinue'
Invoke-WebRequest -Uri $dutchUrl -OutFile $dutchRaw
Write-Host "  Downloaded to: $dutchRaw" -ForegroundColor Gray
Write-Host ""

# Download English dictionary
Write-Host "Downloading English dictionary from dwyl/english-words..." -ForegroundColor Cyan
$englishUrl = "https://raw.githubusercontent.com/dwyl/english-words/master/words_alpha.txt"
$englishRaw = Join-Path $dictionariesPath "english_raw.txt"
Invoke-WebRequest -Uri $englishUrl -OutFile $englishRaw
Write-Host "  Downloaded to: $englishRaw" -ForegroundColor Gray
Write-Host ""

# Sanitize Dutch dictionary
Sanitize-Dictionary -InputFile $dutchRaw -OutputFile (Join-Path $dictionariesPath "dutch.txt") -Language "Dutch"

# Sanitize English dictionary
Sanitize-Dictionary -InputFile $englishRaw -OutputFile (Join-Path $dictionariesPath "english.txt") -Language "English"

# Urban dictionary - sanitize existing
Write-Host "Sanitizing Urban dictionary..." -ForegroundColor Yellow
$urbanFile = Join-Path $dictionariesPath "urban.txt"
if (Test-Path $urbanFile) {
    $urbanWords = Get-Content $urbanFile
    $originalUrbanCount = $urbanWords.Count
    
    # Apply same sanitization rules
    $sanitizedUrban = $urbanWords |
        Where-Object { $_ -match '^[a-zA-Z]+$' } |
        Where-Object { $_.Length -ge 3 -and $_.Length -le 10 } |
        ForEach-Object { $_.ToUpper() } |
        Sort-Object -Unique
    
    $sanitizedUrban | Set-Content $urbanFile -Encoding UTF8
    
    Write-Host "  Original: $originalUrbanCount words" -ForegroundColor Gray
    Write-Host "  Sanitized: $($sanitizedUrban.Count) words" -ForegroundColor Gray
    Write-Host "  Removed: $($originalUrbanCount - $sanitizedUrban.Count) words" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host "  Urban dictionary not found, skipping" -ForegroundColor Gray
    Write-Host ""
}

# Clean up raw files
Write-Host "Cleaning up raw files..." -ForegroundColor Cyan
Remove-Item $dutchRaw -ErrorAction SilentlyContinue
Remove-Item $englishRaw -ErrorAction SilentlyContinue
Write-Host "  Removed temporary raw files" -ForegroundColor Gray
Write-Host ""

# Display final statistics
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
Write-Host "All dictionaries sanitized and ready for use!" -ForegroundColor Green
Write-Host "Words contain only A-Z letters, 3-10 characters in length." -ForegroundColor Gray
