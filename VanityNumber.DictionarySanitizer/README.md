# Dictionary Sanitizer

A .NET library and console tool for sanitizing word dictionaries for use with the Vanity Number API.

## Features

- **Library** (`VanityNumber.DictionarySanitizer`): Reusable class library for dictionary processing
- **Console Tool** (`VanityNumber.DictionarySanitizer.Tool`): Command-line application for easy dictionary generation
- Supports multiple input formats:
  - **Tab-separated**: `word[tab]frequency` (Dutch)
  - **Space-separated**: `word frequency`
  - **One word per line**: `word` (English)
- Filters words to 2-10 characters
- Only includes letters that map to phone digits (A-Z)
- Removes duplicates and sorts alphabetically
- Downloads dictionaries automatically

## Usage

### Console Tool

```bash
# Run with default path (../VanityNumber.Core/Dictionaries)
dotnet run --project VanityNumber.DictionarySanitizer.Tool

# Run with custom path
dotnet run --project VanityNumber.DictionarySanitizer.Tool -- "C:\path\to\dictionaries"
```

### Library

```csharp
using VanityNumber.DictionarySanitizer;

// Sanitize a tab-separated dictionary (word[tab]frequency)
var result = DictionarySanitizer.SanitizeDictionary(
    "dutch_raw.txt",
    DictionaryFormat.TabSeparated);

Console.WriteLine($"Processed {result.SanitizedCount} words from {result.OriginalCount}");

// Or save directly to a file
DictionarySanitizer.SanitizeAndSave(
    "dutch_raw.txt",
    "dutch.txt",
    DictionaryFormat.TabSeparated);
```

## Dictionary Sources

- **Dutch**: [FrequencyWords nl_50k.txt](https://github.com/hermitdave/FrequencyWords) - 50,000 most common Dutch words from subtitle corpus
  - Format: `word[tab]frequency`
  - Output: ~174,818 valid words

- **English**: [dwyl/english-words](https://github.com/dwyl/english-words) - Comprehensive English word list
  - Format: One word per line
  - Output: ~248,437 valid words

## Output Format

The sanitized dictionaries contain:
- One word per line
- Uppercase letters only (A-Z)
- 2-10 characters per word
- Only letters that map to phone keypad (excludes Q, Z in some contexts)
- Alphabetically sorted
- No duplicates

## Building

```bash
# Build library
dotnet build VanityNumber.DictionarySanitizer

# Build console tool
dotnet build VanityNumber.DictionarySanitizer.Tool

# Run tool
dotnet run --project VanityNumber.DictionarySanitizer.Tool
```

## Results

After running the tool:
- `dutch.txt`: 174,818 common Dutch words
- `english.txt`: 248,437 English words
- Both files ready to use with VanityNumber.Core

The dictionaries now contain proper, commonly-used words instead of obscure terms like "LUUTS" or "ANDI".
