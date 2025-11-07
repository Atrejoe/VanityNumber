# Test the Vanity Number API with enum flags

Write-Host "Testing Vanity Number API with DictionaryType Flags" -ForegroundColor Green
Write-Host ""

# Test 1: Using Dutch | English
Write-Host "Test 1: Dutch | English dictionaries" -ForegroundColor Yellow
$response1 = Invoke-RestMethod -Uri "http://localhost:5555/api/VanityNumber/convert/0612345678?dictionaries=Dutch|English&maxResults=3" -Method Get
Write-Host "Original Number: $($response1.originalNumber)"
Write-Host "Matches found: $($response1.matches.Count)"
foreach ($match in $response1.matches) {
    Write-Host "  - $($match.word) (DictionaryType: $($match.dictionaryType), Position: $($match.startPosition), Length: $($match.length))"
}
Write-Host ""

# Test 2: Using only Urban dictionary
Write-Host "Test 2: Urban dictionary only" -ForegroundColor Yellow
$response2 = Invoke-RestMethod -Uri "http://localhost:5555/api/VanityNumber/convert/0612345678?dictionaries=Urban&maxResults=3" -Method Get
Write-Host "Original Number: $($response2.originalNumber)"
Write-Host "Matches found: $($response2.matches.Count)"
foreach ($match in $response2.matches) {
    Write-Host "  - $($match.word) (DictionaryType: $($match.dictionaryType))"
}
Write-Host ""

# Test 3: Using All dictionaries (default)
Write-Host "Test 3: All dictionaries (no parameter)" -ForegroundColor Yellow
$response3 = Invoke-RestMethod -Uri "http://localhost:5555/api/VanityNumber/convert/0612345678?maxResults=5" -Method Get
Write-Host "Original Number: $($response3.originalNumber)"
Write-Host "Matches found: $($response3.matches.Count)"
foreach ($match in $response3.matches) {
    Write-Host "  - $($match.word) (DictionaryType: $($match.dictionaryType))"
}
Write-Host ""

Write-Host "All tests completed!" -ForegroundColor Green
