using VanityNumber.Contracts.Models;
using VanityNumber.Core.Services;

namespace VanityNumber.Tests;

#pragma warning disable CS1591 // Missing XML comment : method should be self explanatory (for now)

/// <summary>
/// Tests for the VanityNumberService that generates vanity numbers from phone numbers.
/// </summary>
public class VanityNumberServiceTests
{
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Component", "VanityNumberService")]
    [Trait("Category", "Integration")]
    [Trait("Component", "VanityNumberService")]
    public void GenerateVanityNumbers_WithValidPhoneNumber_ShouldReturnResults()
    {
        // Arrange
        var letterMapper = new PhoneToLetterMapper();
        var dictionaryService = new DictionaryService();
        var service = new VanityNumberService(letterMapper, dictionaryService);
        
        var request = new VanityNumberRequest
        {
            PhoneNumber = "0612345678",
            DictionaryTypes = DictionaryType.Dutch | DictionaryType.English,
            MinWordLength = 3,
            MaxResults = 10
        };

        // Act
        var result = service.GenerateVanityNumbers(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0612345678", result.OriginalNumber);
        Assert.NotNull(result.Matches);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Component", "VanityNumberService")]
    public void GenerateVanityNumbers_WithEmptyPhoneNumber_ShouldReturnEmptyMatches()
    {
        // Arrange
        var letterMapper = new PhoneToLetterMapper();
        var dictionaryService = new DictionaryService();
        var service = new VanityNumberService(letterMapper, dictionaryService);
        
        var request = new VanityNumberRequest
        {
            PhoneNumber = "",
            MinWordLength = 3,
            MaxResults = 10
        };

        // Act
        var result = service.GenerateVanityNumbers(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Matches);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Component", "VanityNumberService")]
    public void GenerateVanityNumbers_WithPhoneNumberContainingNonDigits_ShouldCleanAndProcess()
    {
        // Arrange
        var letterMapper = new PhoneToLetterMapper();
        var dictionaryService = new DictionaryService();
        var service = new VanityNumberService(letterMapper, dictionaryService);
        
        var request = new VanityNumberRequest
        {
            PhoneNumber = "555-123-4567", // 10 digits: 5551234567
            DictionaryTypes = DictionaryType.English,
            MinWordLength = 3,
            MaxResults = 10
        };

        // Act
        var result = service.GenerateVanityNumbers(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("555-123-4567", result.OriginalNumber);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Component", "VanityNumberService")]
    public void GenerateVanityNumbers_WithDefaultDictionaryTypes_ShouldUseAllDictionaries()
    {
        // Arrange
        var letterMapper = new PhoneToLetterMapper();
        var dictionaryService = new DictionaryService();
        var service = new VanityNumberService(letterMapper, dictionaryService);
        
        var request = new VanityNumberRequest
        {
            PhoneNumber = "0612345678",
            // DictionaryTypes defaults to All
            MinWordLength = 3,
            MaxResults = 10
        };

        // Act
        var result = service.GenerateVanityNumbers(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Matches);
    }

    [Theory]
    [Trait("Category", "Integration")]
    [Trait("Component", "VanityNumberService")]
    [InlineData(3, 10)]
    [InlineData(4, 5)]
    [InlineData(5, 20)]
    public void GenerateVanityNumbers_WithDifferentParameters_ShouldRespectLimits(int minWordLength, int maxResults)
    {
        // Arrange
        var letterMapper = new PhoneToLetterMapper();
        var dictionaryService = new DictionaryService();
        var service = new VanityNumberService(letterMapper, dictionaryService);
        
        var request = new VanityNumberRequest
        {
            PhoneNumber = "0612345678",
            DictionaryTypes = DictionaryType.English,
            MinWordLength = minWordLength,
            MaxResults = maxResults
        };

        // Act
        var result = service.GenerateVanityNumbers(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Matches.Count <= maxResults);
        
        // All matches should have words at least minWordLength long
        foreach (var match in result.Matches)
        {
            Assert.True(match.Word.Length >= minWordLength);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Component", "VanityNumberService")]
    public void GenerateVanityNumbers_ShouldOrderByWordLength()
    {
        // Arrange
        var letterMapper = new PhoneToLetterMapper();
        var dictionaryService = new DictionaryService();
        var service = new VanityNumberService(letterMapper, dictionaryService);
        
        var request = new VanityNumberRequest
        {
            PhoneNumber = "0612345678",
            DictionaryTypes = DictionaryType.English,
            MinWordLength = 3,
            MaxResults = 20
        };

        // Act
        var result = service.GenerateVanityNumbers(request);

        // Assert
        if (result.Matches.Count > 1)
        {
            for (int i = 0; i < result.Matches.Count - 1; i++)
            {
                // Longer or equal length words should come first
                Assert.True(result.Matches[i].Length >= result.Matches[i + 1].Length);
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Component", "VanityNumberService")]
    public void GenerateVanityNumbers_WithFlagsCombination_ShouldSearchMultipleDictionaries()
    {
        // Arrange
        var letterMapper = new PhoneToLetterMapper();
        var dictionaryService = new DictionaryService();
        var service = new VanityNumberService(letterMapper, dictionaryService);
        
        var request = new VanityNumberRequest
        {
            PhoneNumber = "0612345678",
            DictionaryTypes = DictionaryType.Dutch | DictionaryType.Urban, // Combining flags
            MinWordLength = 3,
            MaxResults = 10
        };

        // Act
        var result = service.GenerateVanityNumbers(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Matches);
    }

    [Theory]
    [Trait("Category", "Integration")]
    [Trait("Component", "VanityNumberService")]
    [InlineData("739", "SEX", DictionaryType.English)]        // 7=PQRS, 3=DEF, 9=WXYZ
    [InlineData("2273", "CARE", DictionaryType.English)]      // 2=ABC, 2=ABC, 7=PQRS, 3=DEF
    [InlineData("2273", "BASE", DictionaryType.English)]      // Alternative word for same number
    [InlineData("4663", "HOME", DictionaryType.English)]      // 4=GHI, 6=MNO, 6=MNO, 3=DEF
    [InlineData("2665", "BOOK", DictionaryType.English)]      // 2=ABC, 6=MNO, 6=MNO, 5=JKL
    [InlineData("2255", "CALL", DictionaryType.English)]      // 2=ABC, 2=ABC, 5=JKL, 5=JKL
    [InlineData("4357", "HELP", DictionaryType.English)]      // 4=GHI, 3=DEF, 5=JKL, 7=PQRS
    [InlineData("5683", "LOVE", DictionaryType.English)]      // 5=JKL, 6=MNO, 8=TUV, 3=DEF
    [InlineData("9673", "WORD", DictionaryType.English)]      // 9=WXYZ, 6=MNO, 7=PQRS, 3=DEF
    [InlineData("2665", "COOL", DictionaryType.English)]      // 2=ABC, 6=MNO, 6=MNO, 5=JKL
    [InlineData("3663", "FOOD", DictionaryType.English)]      // 3=DEF, 6=MNO, 6=MNO, 3=DEF
    [InlineData("5433", "LIFE", DictionaryType.English)]      // 5=JKL, 4=GHI, 3=DEF, 3=DEF
    [InlineData("2437", "BIER", DictionaryType.Dutch)]        // 2=ABC, 4=GHI, 3=DEF, 7=PQRS (Dutch for "beer")
    [InlineData("4847", "HUIS", DictionaryType.Dutch)]        // 4=GHI, 8=TUV, 4=GHI, 7=PQRS (Dutch for "house")
    [InlineData("4663", "HOME", DictionaryType.Dutch | DictionaryType.English)]  // Test multi-dictionary flags
    public void GenerateVanityNumbers_WithKnownPhoneNumbers_ShouldReturnExpectedWords(string phoneNumber, string expectedWord, DictionaryType dictionaryType)
    {
        // Arrange
        var letterMapper = new PhoneToLetterMapper();
        var dictionaryService = new DictionaryService();
        var service = new VanityNumberService(letterMapper, dictionaryService);
        
        var request = new VanityNumberRequest
        {
            PhoneNumber = phoneNumber,
            DictionaryTypes = dictionaryType,
            MinWordLength = 3,
            MaxResults = 50
        };

        // Act
        var result = service.GenerateVanityNumbers(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(phoneNumber, result.OriginalNumber);
        Assert.NotEmpty(result.Matches);
        
        // Verify that the expected word is among the matches
        var match = result.Matches.FirstOrDefault(m => m.Word.Equals(expectedWord, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(match);
        Assert.Equal(expectedWord, match.Word, ignoreCase: true);
        Assert.True(match.DictionaryType.HasFlag(dictionaryType));
        Assert.Equal(0, match.StartPosition);
        Assert.Equal(expectedWord.Length, match.Length);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "VanityNumberService")]
    public void GenerateVanityNumbers_WithPhoneNumberTooLong_ShouldThrowException()
    {
        // Arrange
        var letterMapper = new PhoneToLetterMapper();
        var dictionaryService = new DictionaryService();
        var service = new VanityNumberService(letterMapper, dictionaryService);
        
        var request = new VanityNumberRequest
        {
            PhoneNumber = "12345678901", // 11 digits - too long
            DictionaryTypes = DictionaryType.English,
            MinWordLength = 3,
            MaxResults = 10
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => service.GenerateVanityNumbers(request));
        Assert.Contains("at most 10 digits", exception.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "VanityNumberService")]
    public void GenerateVanityNumbers_WithPhoneNumberTooShort_ShouldThrowException()
    {
        // Arrange
        var letterMapper = new PhoneToLetterMapper();
        var dictionaryService = new DictionaryService();
        var service = new VanityNumberService(letterMapper, dictionaryService);
        
        var request = new VanityNumberRequest
        {
            PhoneNumber = "12", // 2 digits - too short
            DictionaryTypes = DictionaryType.English,
            MinWordLength = 3,
            MaxResults = 10
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => service.GenerateVanityNumbers(request));
        Assert.Contains("at least 3 digits", exception.Message);
    }
}
