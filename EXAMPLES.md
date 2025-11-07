# Example API Requests

## Using PowerShell

### Example 1: Convert with POST (all dictionaries)
```powershell
$body = @{
    phoneNumber = "0612345678"
    minWordLength = 3
    maxResults = 20
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/VanityNumber/convert" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

### Example 2: Convert with POST (Dutch only)
```powershell
$body = @{
    phoneNumber = "0612345678"
    dictionaryTypes = @("Dutch")
    minWordLength = 3
    maxResults = 10
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/VanityNumber/convert" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

### Example 3: Simple GET request
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/VanityNumber/convert/0612345678"
```

### Example 4: GET with specific dictionaries
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/VanityNumber/convert/0612345678?dictionaries=Dutch,English"
```

## Using cURL

### POST request
```bash
curl -X POST "http://localhost:5000/api/VanityNumber/convert" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "0612345678",
    "dictionaryTypes": ["Dutch", "English"],
    "minWordLength": 3,
    "maxResults": 20
  }'
```

### GET request
```bash
curl "http://localhost:5000/api/VanityNumber/convert/0612345678?dictionaries=Dutch"
```

## Expected Response Format

```json
{
  "originalNumber": "0612345678",
  "matches": [
    {
      "vanityNumber": "06BEI45678",
      "word": "BEI",
      "dictionaryType": "Dutch",
      "startPosition": 2,
      "length": 3
    },
    {
      "vanityNumber": "061CHILD78",
      "word": "CHILD",
      "dictionaryType": "English",
      "startPosition": 3,
      "length": 5
    }
  ]
}
```

## Phone Number Mappings

The API uses standard phone keypad mappings:
- 2 → ABC
- 3 → DEF
- 4 → GHI
- 5 → JKL
- 6 → MNO
- 7 → PQRS
- 8 → TUV
- 9 → WXYZ

So "234" could map to words like:
- ADG, ADH, ADI
- AEG, AEH, AEI
- AFG, AFH, AFI
- BDG, BDH, BDI
- BEG, BEH, BEI ✓ (if "BEI" is in dictionary)
- etc.
