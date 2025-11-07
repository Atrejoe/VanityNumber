# Downloads dictionary files into VanityNumberApi.Core/Dictionaries
# Update the $dictionaryUrls values with the actual URLs if needed

$dictionaryDir = "VanityNumberApi.Core/Dictionaries"
$dictionaryUrls = @{
    "dutch.txt"   = "https://example.com/dutch.txt"
    "english.txt" = "https://example.com/english.txt"
    "urban.txt"   = "https://example.com/urban.txt"
}

if (!(Test-Path $dictionaryDir)) {
    New-Item -ItemType Directory -Path $dictionaryDir | Out-Null
}

foreach ($dict in $dictionaryUrls.Keys) {
    $url = $dictionaryUrls[$dict]
    $dest = Join-Path $dictionaryDir $dict
    Write-Host "Downloading $dict from $url ..."
    Invoke-WebRequest -Uri $url -OutFile $dest
}

Write-Host "All dictionaries downloaded to $dictionaryDir"