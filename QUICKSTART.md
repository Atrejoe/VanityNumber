# Vanity Number API - Quick Start Guide

## Overview
✅ A complete .NET 10 Web API has been created that converts phone numbers to vanity numbers using multiple dictionaries.

## What Was Built

### Core Features
- **Phone-to-Letter Mapping**: Standard phone keypad (1=none, 2=ABC, 3=DEF, etc.)
- **Multi-Dictionary Support**: 
  - Dutch (OpenTaal: ~400k+ words)
  - English (~370k+ words)
  - Urban Dictionary (~170+ slang terms)
- **REST API Endpoints**: Both GET and POST methods
- **Swagger UI**: Interactive API documentation
- **Configurable Options**: Min word length, max results, dictionary selection

### Project Structure
```
naamnummers/
├── VanityNumberApi/              # Main project folder
│   ├── Controllers/
│   │   └── VanityNumberController.cs    # API endpoints
│   ├── Services/
│   │   ├── PhoneToLetterMapper.cs       # Digit to letter conversion
│   │   ├── DictionaryService.cs         # Word lookup service
│   │   └── VanityNumberService.cs       # Main conversion logic
│   ├── Models/
│   │   └── VanityNumber.cs              # Request/Response models
│   ├── Dictionaries/
│   │   ├── dutch.txt                    # Dutch word list (OpenTaal)
│   │   ├── english.txt                  # English word list
│   │   └── urban.txt                    # Urban dictionary slang
│   ├── Program.cs                        # App configuration
│   ├── VanityNumberApi.csproj           # Project file (.NET 10)
│   └── appsettings.json                 # Settings
└── Documentation:
    ├── README.md                         # Full documentation
    ├── EXAMPLES.md                       # API usage examples
    └── QUICKSTART.md                     # This file
```

## Running the Application

### Method 1: Using dotnet CLI

1. **Navigate to project directory:**

   ``` powershell
   cd c:\Users\rsirre\Desktop\naamnummers\VanityNumberApi
   ```

2. **Run the application:**

   ```powershell
   dotnet run
   ```

3. **Access the API:**
   - Swagger UI: http://localhost:5000/swagger
   - HTTPS: https://localhost:5001/swagger

### Method 2: Using Visual Studio Code

1. Open the folder in VS Code
2. Press F5 or use the "Run and Debug" panel
3. Select ".NET Core Launch (web)"
4. The browser will open automatically with Swagger UI

### Method 3: Using Visual Studio

1. Double-click `naamnummers.sln` (if it exists in the folder)
2. Press F5 to run
3. Navigate to the Swagger UI URL

## API Endpoints

### POST /api/VanityNumber/convert

#### Full-featured conversion with all options

```json
POST http://localhost:5000/api/VanityNumber/convert
Content-Type: application/json

{
  "phoneNumber": "0612345678",
  "dictionaryTypes": ["Dutch", "English"],
  "minWordLength": 3,
  "maxResults": 20
}
```

### GET /api/VanityNumber/convert/{phoneNumber}

#### Quick conversion with query parameters

```
GET http://localhost:5000/api/VanityNumber/convert/0612345678?dictionaries=Dutch
```

## Example Usage

### PowerShell Example

```powershell
# Test the API once it's running
$body = @{
    phoneNumber = "0612345678"
    dictionaryTypes = @("Dutch")
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/VanityNumber/convert" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

### Expected Response

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

## How It Works

### Phone Keypad Mapping

```
1 = (no letters)
2 = ABC
3 = DEF
4 = GHI
5 = JKL
6 = MNO
7 = PQRS
8 = TUV
9 = WXYZ
0 = (no letters)
```

### Example: 0612345678
1. **Input**: `0612345678`
2. **Segment**: `234` (positions 2-4)
3. **Possible Letters**: 
   - 2 → A, B, C
   - 3 → D, E, F
   - 4 → G, H, I
4. **Combinations**: ADG, ADH, ADI, AEG, AEH, AEI, AFG, AFH, AFI, BDG, BDH, BDI, BEG, BEH, **BEI** ✓, etc.
5. **Dictionary Match**: "BEI" found in Dutch dictionary
6. **Result**: `06BEI45678`

## Customization

### Adding More Words

Simply edit the dictionary files in the `VanityNumberApi/Dictionaries/` folder:

**VanityNumberApi/Dictionaries/dutch.txt**
```
HUIS
AUTO
TELEFOON
YOUR_NEW_WORD_HERE
```

One word per line, then restart the API.

### Adjusting Search Parameters

In your API request:
- `minWordLength`: Minimum letters in a word (default: 3)
- `maxResults`: Maximum matches to return (default: 20)
- `dictionaryTypes`: Which dictionaries to use

## Troubleshooting

### Port Already in Use
If port 5000/5001 is busy, modify `launchSettings.json` or use:
```powershell
dotnet run --urls "http://localhost:5555"
```

### No Matches Found
- Check that dictionary files exist in `VanityNumberApi/Dictionaries/` folder
- Ensure words are in UPPERCASE in dictionary files
- Try reducing `minWordLength` to 2 or 3
- Verify phone number has enough digits

### Permission Errors

If you get "Access is denied" errors:

1. Try running VS Code or terminal as Administrator
2. Check antivirus isn't blocking the executable
3. Rebuild the project: `dotnet clean && dotnet build`

## Testing with Swagger UI

1. Start the application
2. Navigate to http://localhost:5000/swagger
3. Click on "POST /api/VanityNumber/convert"
4. Click "Try it out"
5. Enter your request JSON
6. Click "Execute"
7. See the results below

## Next Steps

✅ **The API is ready to use!**

1. Start the application using one of the methods above
2. Test with Swagger UI or PowerShell
3. Add more words to the dictionaries as needed
4. Customize the logic in the Services folder

## File Locations

- **Main Code**: `c:\Users\rsirre\Desktop\naamnummers\VanityNumberApi\`
- **Dictionaries**: `c:\Users\rsirre\Desktop\naamnummers\VanityNumberApi\Dictionaries\`
- **Documentation**: 
  - `README.md` - Complete documentation
  - `EXAMPLES.md` - API usage examples
  - `QUICKSTART.md` - This file

---

**Built with .NET 10 Web API | Supports Dutch, English & Urban Dictionaries**
