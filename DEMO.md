# Vanity Number API - New Features Demo

## Test Scenarios

### 1. Leet Speak Example: "BOSS"

Phone Number: `8055`

- 8 = **B** (leet speak)
- 0 = **O** (leet speak)  
- 5 = **S** (leet speak)
- 5 = **S** (leet speak)
Result: **BOSS**

### 2. Partial Match Example

Phone Number: `17398`

- Standard match at position 1-3: `73` → SE
- **Partial match at position 2-4:** `739` → **SEX**
Result: `1-SEX-8` (match not at start!)

### 3. Longer Matches Prioritized

Phone Number: `4663`
Possible matches:

- `HOME` (4 letters) ✅ **Shown first**
- `HON` (3 letters) - Shown after
- `GOO` (3 letters) - Shown after

### 4. Phone Number Validation

✅ Valid: `555-123-4567` (10 digits)
✅ Valid: `+31 612 345` (10 digits: 3161234 5)
❌ Invalid: `12` (too short, < 3 digits)
❌ Invalid: `12345678901` (too long, > 10 digits)

### 5. Dictionary Quality

Before sanitization:

- Dutch: 413,937 words (included numbers like "010", words with diacritics)
- English: 370,105 words (included non-standard entries like "RFZ")

After sanitization:

- Dutch: 176,573 words (**A-Z only, 3-10 chars**)
- English: 248,010 words (**A-Z only, 3-10 chars**)

## Quick API Test

```powershell
# Test leet speak "BOSS"
$body = @{
    phoneNumber = '8055'
    dictionaryTypes = 2  # English
    minWordLength = 3
    maxResults = 10
} | ConvertTo-Json

Invoke-RestMethod -Uri 'http://localhost:65033/api/vanitynumber' `
    -Method POST `
    -Body $body `
    -ContentType 'application/json'
```

Expected result:

```json
{
  "originalNumber": "8055",
  "matches": [
    {
      "vanityNumber": "BOSS",
      "word": "BOSS",
      "dictionaryType": 2,
      "startPosition": 0,
      "length": 4
    },
    // ... more matches
  ]
}
```

## Coverage Summary

**Overall:** 95.5% line coverage, 89.7% branch coverage  
**Tests:** 48 passing (Unit + Integration + Leet Speak)  
**Report:** See `COVERAGE_REPORT.md` and `VanityNumberApi.Tests/TestResults/CoverageReport/index.html`
