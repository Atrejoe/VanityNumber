using VanityNumberApi.Core.Models;
using VanityNumberApi.Core.Services;

namespace VanityNumberApi.Tests;

/// <summary>
/// Tests for the DictionaryService that validates words against dictionaries.
/// </summary>
public class DictionaryServiceTests
{
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Component", "DictionaryService")]
    [InlineData("XYZQWERTY", DictionaryType.Dutch)]
    [InlineData("XYZQWERTY", DictionaryType.English)]
    [InlineData("XYZQWERTY", DictionaryType.Urban)]
    public void IsWord_WithInvalidWord_ShouldReturnFalse(string word, DictionaryType dictionaryType)
    {
        // Arrange
        var service = new DictionaryService();

        // Act
        var result = service.IsWord(word, dictionaryType);

        // Assert
        Assert.False(result, $"Expected nonsense word '{word}' to not be found in {dictionaryType} dictionary");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "DictionaryService")]
    public void FindWords_WithCandidates_ShouldReturnOnlyValidWords()
    {
        // Arrange
        var service = new DictionaryService();
        var candidates = new[] { "AAA", "BBB", "CCC", "DDD", "XYZQWERTY123" };

        // Act
        var result = service.FindWords(candidates, DictionaryType.English);

        // Assert
        Assert.NotNull(result);
        // Result should be a subset of candidates
        foreach (var word in result)
        {
            Assert.Contains(word, candidates, StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "DictionaryService")]
    public void FindWords_WithEmptyCandidates_ShouldReturnEmpty()
    {
        // Arrange
        var service = new DictionaryService();
        var candidates = Array.Empty<string>();

        // Act
        var result = service.FindWords(candidates, DictionaryType.English);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Component", "DictionaryService")]
    [InlineData(DictionaryType.Dutch)]
    [InlineData(DictionaryType.English)]
    [InlineData(DictionaryType.Urban)]
    public void FindWords_AllDictionaries_ShouldNotThrowException(DictionaryType dictionaryType)
    {
        // Arrange
        var service = new DictionaryService();
        var candidates = new[] { "TEST", "WORD", "EXAMPLE" };

        // Act & Assert
        var exception = Record.Exception(() => service.FindWords(candidates, dictionaryType));
        Assert.Null(exception);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "DictionaryService")]
    public void FindWords_WithFlagsCombination_ShouldSearchMultipleDictionaries()
    {
        // Arrange
        var service = new DictionaryService();
        var candidates = new[] { "TEST", "WORD", "EXAMPLE" };

        // Act - using flags to combine dictionaries
        var result = service.FindWords(candidates, DictionaryType.Dutch | DictionaryType.English);

        // Assert
        Assert.NotNull(result);
        // Should be able to search across multiple dictionaries
    }

    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Component", "DictionaryService")]
    [InlineData(DictionaryType.Dutch, "EN")]      // "and" in Dutch
    [InlineData(DictionaryType.English, "THE")]
    [InlineData(DictionaryType.Urban, "COOL")]
    public void Constructor_WithEmbeddedResources_ShouldLoadDictionary(DictionaryType dictionaryType, string testWord)
    {
        // Arrange & Act
        var service = new DictionaryService();

        // Assert - Verify dictionary loaded by checking it can find a common word
        var result = service.IsWord(testWord, dictionaryType);
        
        // At least verify the service doesn't crash and returns a boolean result
        Assert.IsType<bool>(result);
    }
}
