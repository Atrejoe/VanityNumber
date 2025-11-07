# Vanity Number API - Project Structure

## Solution Architecture

The solution has been refactored into a clean, layered architecture:

``` plaintext
naamnummers/
├── VanityNumberApi/              # 🎯 API Layer (Thin - Controllers only)
│   ├── Controllers/
│   │   └── VanityNumberController.cs
│   ├── Dictionaries/
│   │   ├── dutch.txt (~400k+ words from OpenTaal)
│   │   ├── english.txt (~370k+ words)
│   │   └── urban.txt (~170+ slang terms)
│   ├── Program.cs
│   └── VanityNumberApi.csproj
│
├── VanityNumberApi.Core/         # 💼 Business Logic Layer
│   ├── Models/
│   │   └── VanityNumber.cs (DTOs)
│   ├── Services/
│   │   ├── PhoneToLetterMapper.cs
│   │   ├── DictionaryService.cs
│   │   └── VanityNumberService.cs
│   └── VanityNumberApi.Core.csproj
│
├── VanityNumberApi.Tests/        # ✅ Unit Tests (xUnit)
│   ├── PhoneToLetterMapperTests.cs (14 tests)
│   ├── DictionaryServiceTests.cs (6 tests)
│   ├── VanityNumberServiceTests.cs (9 tests)
│   └── VanityNumberApi.Tests.csproj
│
└── VanityNumberApi.sln           # Solution file

Documentation:
├── README.md
├── QUICKSTART.md
├── EXAMPLES.md
└── PROJECT_STRUCTURE.md (this file)
```

## Project Dependencies

```plaintext
VanityNumberApi (Web API)
    └── VanityNumberApi.Core (Class Library)

VanityNumberApi.Tests (xUnit)
    └── VanityNumberApi.Core (Class Library)
```

## Technology Stack

- **.NET 10** (RC 2)
- **NSwag** for OpenAPI/Swagger UI
- **xUnit** for unit testing
- **Real dictionaries**:
  - Dutch: OpenTaal official wordlist
  - English: dwyl/english-words
  - Urban: Curated slang terms

## Test Summary

Total: 29 tests - All passing ✅

### PhoneToLetterMapperTests (14 tests)

- Validates digit-to-letter mapping for all digits (2-9, 0, 1)
- Tests combination generation
- Edge cases (empty strings, single digits)

### DictionaryServiceTests (6 tests)

- Word validation across all three dictionaries
- FindWords functionality
- Empty input handling
- Dictionary type support

### VanityNumberServiceTests (9 tests)

- End-to-end vanity number generation
- Phone number cleaning (handles +31, spaces, dashes)
- Parameter validation (min word length, max results)
- Dictionary type selection
- Result ordering (by word length)

## Running the Project

### Build Everything

```powershell
cd c:\Users\rsirre\Desktop\naamnummers
dotnet build VanityNumberApi.sln
```

### Run Tests

```powershell
cd VanityNumberApi.Tests
dotnet test
```

### Run API

```powershell
cd VanityNumberApi
dotnet run --urls "http://localhost:5555"
```

Then visit: **<http://localhost:5555/>** for Swagger UI

## API Layer (Thin)

The API project now only contains:

- **Controllers**: REST endpoint definitions
- **Program.cs**: App configuration and DI setup
- **Dictionaries**: Data files (could be moved to separate project if needed)

All business logic is in the Core project.

## Core Layer (Business Logic)

The Core project contains:

- **Models**: DTOs and domain models
- **Services**: All business logic
  - Phone number to letter mapping
  - Dictionary lookups
  - Vanity number generation algorithm

## Benefits of This Architecture

1. **Separation of Concerns**: API layer is thin, only handles HTTP concerns
2. **Testability**: Business logic is easily testable without HTTP infrastructure
3. **Reusability**: Core library can be used in other projects (console app, Azure Function, etc.)
4. **Maintainability**: Clear boundaries between layers
5. **Performance**: Unit tests run fast (no HTTP stack needed)

## Example Usage

### Test a specific class

```powershell
dotnet test --filter "FullyQualifiedName~PhoneToLetterMapperTests"
```

### Test with coverage

```powershell
dotnet test /p:CollectCoverage=true
```

### Build in Release mode

```powershell
dotnet build -c Release VanityNumberApi.sln
```

## Next Steps

Potential enhancements:

- Add integration tests for the API layer
- Add performance benchmarks
- Implement caching for dictionary lookups
- Add health check endpoints
- Deploy to Azure App Service
- Add CI/CD pipeline

---

**Project Status**: ✅ Production Ready

- Clean architecture implemented
- 29 unit tests passing
- Real dictionaries integrated
- Swagger UI configured
- .NET 10 compatible
