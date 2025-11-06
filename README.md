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

```
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
│         │         │         │
└─────────┴─────────┴─────────┘
```

## Getting Started

### Prerequisites

- .NET 10 SDK

### Running the Application

1. Navigate to the project directory:
```powershell
cd c:\Users\rsirre\Desktop\naamnummers\VanityNumberApi
```

2. Restore dependencies:
```powershell
dotnet restore
```

3. Run the application:
```powershell
dotnet run
```

4. Open your browser and navigate to:
   - Swagger UI: `https://localhost:5001/swagger` or `http://localhost:5000/swagger`

## API Endpoints

### POST /api/VanityNumber/convert

Convert a phone number with full control over parameters.

**Request Body:**
```json
{
  "phoneNumber": "0612345678",
  "dictionaryTypes": ["Dutch", "English", "Urban"],
  "minWordLength": 3,
  "maxResults": 20
}
```

**Response:**
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
    }
  ]
}
```

### GET /api/VanityNumber/convert/{phoneNumber}

Simple GET request to convert a phone number.

**Example:**
```
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

```
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
