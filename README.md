# Vanity Number API

A .NET 10 Web API that converts phone numbers into vanity numbers using multiple dictionaries (Dutch, English, and Urban Dictionary).

## Features

- Converts 10-digit phone numbers to vanity numbers
- Supports multiple dictionaries:
  - **Dutch**: OpenTaal official Dutch word list (~400k+ words)
  - **English**: Comprehensive English dictionary (~370k+ words)
  - **Urban**: Popular slang and internet culture terms
- Phone keypad mapping (standard T9 layout)
- REST API with both GET and POST endpoints
- Swagger UI for easy testing

## Phone Keypad Mapping

The API uses standard T9 phone keypad mapping with **enhanced leet speak support**:

### Standard T9 Mapping

``` txt
┌─────────┬─────────┬─────────┐
│    1    │    2    │    3    │
│         │   ABC   │   DEF   │
├─────────┼─────────┼─────────┤
│    4    │    5    │    6    │
│   GHI   │   JKL   │   MNO   │
├─────────┼─────────┼─────────┤
│    7    │    8    │    9    │
│   PQRS  │   TUV   │   WXYZ  │
├─────────┼─────────┼─────────┤
│    *    │    0    │    #    │
│         │    O    │         │
└─────────┴─────────┴─────────┘
```

### Leet Speak Enhancement

The API includes leet speak mappings to increase matching possibilities:

| Digit | Standard Letters | Leet Speak Alternative | Combined Mapping |
|-------|-----------------|------------------------|------------------|
| 0     | -               | O                      | O                |
| 1     | -               | I, L                   | I, L             |
| 2     | ABC             | -                      | ABC              |
| 3     | DEF             | E (included)           | DEF              |
| 4     | GHI             | A                      | GHIA             |
| 5     | JKL             | S                      | JKLS             |
| 6     | MNO             | -                      | MNO              |
| 7     | PQRS            | T (included)           | PQRST            |
| 8     | TUV             | B                      | TUVB             |
| 9     | WXYZ            | -                      | WXYZ             |

**Example:** The phone number `828626` can match the word "BATMAN":

- Position 0: `8` → **B** (leet speak)
- Position 1: `2` → **A** (standard)
- Position 2: `8` → **T** (standard)
- Position 3: `6` → **M** (standard)
- Position 4: `2` → **A** (standard)
- Position 5: `6` → **N** (standard)

**Important:** Leet speak mappings only apply to the **original digits** in the phone number. For example, the digit `4` can map to the letter `A` (leet), but the letter `A` will only appear in positions where the phone number has a `4`, not where it has other digits like `2`.

**Vanity Display Format**: When displaying matches, digits where leet speak was used are shown as the original digit, while standard mappings are shown as lowercase letters. This ensures that converting the vanity display back to a phone number preserves the original number.

**Example:** The phone number `828646` can match "batman":

- Position 0: `8` → `8` displayed (used leet 8→B)
- Position 1: `2` → `a` displayed (standard 2→A)
- Position 2: `8` → `t` displayed (standard 8→T)
- Position 3: `6` → `m` displayed (standard 6→M)
- Position 4: `4` → `4` displayed (used leet 4→A)
- Position 5: `6` → `n` displayed (standard 6→N)

**Result:** `8atm4n` - Converting back: `828646` (preserves original number)

### Diacritic Matching

The API provides intelligent matching for words with diacritics (accents and special characters):

- **Matching**: Words are matched using **normalized** (without diacritics) forms for flexibility
- **Display**: Matched words are displayed in their **original** form with diacritics preserved

**Dictionary Format**: Each line contains: `NORMALIZED[TAB]original`

**Examples:**

``` txt
CAFE      café       # Matches phone input, displays as "café"
AANGEERFD aangeërfd  # Dutch word with diaeresis preserved
RESUME    résumé     # Matches "RESUME", displays as "résumé"
```

**How it works:**

1. User enters phone number: `2233`
2. System generates combinations: `CAFE`, `BAED`, etc.
3. System normalizes and checks dictionary: `CAFE` → found as `CAFE	café`
4. System returns match with diacritics: "café"

This ensures maximum matching flexibility while preserving the cultural authenticity of words from different languages, particularly important for Dutch words with diaereses (ë, ï, ö, etc.).

## Getting Started

### Prerequisites

- .NET 10 SDK

### Running the Application

1. Restore dependencies

```powershell
dotnet restore
```

2. Run the application:

```powershell
dotnet run
```

3. Open your browser and navigate to:
   - Swagger UI: `https://localhost:5001/swagger` or `http://localhost:5000/swagger`

## API Endpoints

### POST /api/VanityNumber/convert

Convert a phone number with full control over parameters.

**Request Body:**

``` json
{
  "phoneNumber": "0612345678",
  "dictionaryTypes": ["Dutch", "English", "Urban"],
  "minWordLength": 3,
  "maxResults": 20
}
```

**Response:**

``` json
{
  "originalNumber": "0612345678",
  "matches": [
    {
      "vanityNumber": "06BEI45678",
      "word": "BEI",
      "dictionaryType": "Dutch",
      "startPosition": 2,
      "length": 3
    }
  ]
}
```

### GET /api/VanityNumber/convert/{phoneNumber}

Simple GET request to convert a phone number.

**Example:**

``` bash
GET /api/VanityNumber/convert/0612345678?dictionaries=Dutch,English&minWordLength=3&maxResults=20
```

**Query Parameters:**

- `dictionaries` (optional): Comma-separated list (Dutch,English,Urban). Default: all
- `minWordLength` (optional): Minimum word length. Default: 3
- `maxResults` (optional): Maximum results to return. Default: 20

## Example Usage

### Using cURL (PowerShell)

```powershell
# POST request
Invoke-RestMethod -Uri "https://localhost:5001/api/VanityNumber/convert" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"phoneNumber":"0612345678","dictionaryTypes":["Dutch"]}'

# GET request
Invoke-RestMethod -Uri "https://localhost:5001/api/VanityNumber/convert/0612345678?dictionaries=Dutch"
```

### Example with Your Number

For the Dutch number `0612345678`, the API will find words like:

- `06BEI45678` (where BEI is position 2-4, using digits 234)
- And other matches based on the dictionary words

## Dictionary Files

The API uses three comprehensive dictionary files located in the `VanityNumberApi/Dictionaries` folder:

- **dutch.txt** - Official Dutch word list from OpenTaal (~400,000+ words)
  - Source: https://github.com/OpenTaal/opentaal-wordlist
  - Words filtered to 3-15 characters, converted to uppercase

- **english.txt** - Comprehensive English dictionary (~370,000+ words)
  - Source: https://github.com/dwyl/english-words
  - Words filtered to 3-15 characters, converted to uppercase

- **urban.txt** - Popular slang and internet culture terms (~170+ curated terms)
  - Includes modern slang, memes, and internet culture vocabulary
  - Manually curated for relevance

You can add more words to these files (one word per line, uppercase) and restart the application.

## Project Structure

``` txt
VanityNumberApi/
├── Controllers/
│   └── VanityNumberController.cs
├── Services/
│   ├── PhoneToLetterMapper.cs
│   ├── DictionaryService.cs
│   └── VanityNumberService.cs
├── Models/
│   └── VanityNumber.cs
├── Dictionaries/
│   ├── dutch.txt
│   ├── english.txt
│   └── urban.txt
├── Program.cs
├── VanityNumberApi.csproj
└── appsettings.json
```

## How It Works

1. **Phone Number Cleaning**: Removes non-digit characters
2. **Letter Mapping**: Maps each digit to possible letters (2=ABC, 3=DEF, etc.)
3. **Combination Generation**: Generates all possible letter combinations for segments
4. **Dictionary Lookup**: Checks combinations against selected dictionaries
5. **Result Formatting**: Returns matches with the original number format

## Customization

### Adding More Words

Edit the dictionary files in the `Dictionaries` folder and add one word per line.

### Adding New Dictionaries

1. Create a new text file in `Dictionaries/`
2. Add the dictionary type to `DictionaryType` enum in `DictionaryService.cs`
3. Load the new dictionary in the `DictionaryService` constructor
4. Update the `GetDictionary` method

## License

MIT
