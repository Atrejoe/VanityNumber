using static VanityNumberApi.DictionarySanitizer.DictionarySanitizer;

namespace VanityNumberApi.DictionarySanitizer.Tests;

/// <summary>
/// Tests for the DictionarySanitizer class that processes dictionary files.
/// </summary>
public class DictionarySanitizerTests
{
    private readonly string _testFilesDirectory;

    public DictionarySanitizerTests()
    {
        _testFilesDirectory = Path.Combine(Path.GetTempPath(), "DictionarySanitizerTests");
        Directory.CreateDirectory(_testFilesDirectory);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SanitizeDictionary_WithTabSeparatedFormat_ShouldParseCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testFilesDirectory, "test_tab.txt");
        File.WriteAllText(testFile, "café\t1000\nnaïve\t500\nhello\t2000");
        
        // Act
        var result = DictionarySanitizer.SanitizeDictionary(testFile, DictionaryFormat.TabSeparated);

        // Assert
        Assert.Equal(3, result.SanitizedCount);
        Assert.Contains("CAFE\tcafé", result.Lines);
        Assert.Contains("NAIVE\tnaïve", result.Lines);
        Assert.Contains("HELLO\thello", result.Lines);
        
        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SanitizeDictionary_WithOneWordPerLine_ShouldParseCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testFilesDirectory, "test_oneline.txt");
        File.WriteAllText(testFile, "café\nnaïve\nhello");
        
        // Act
        var result = DictionarySanitizer.SanitizeDictionary(testFile, DictionaryFormat.OneWordPerLine);

        // Assert
        Assert.Equal(3, result.SanitizedCount);
        Assert.Contains("CAFE\tcafé", result.Lines);
        Assert.Contains("NAIVE\tnaïve", result.Lines);
        Assert.Contains("HELLO\thello", result.Lines);
        
        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SanitizeDictionary_ShouldFilterOutTooShortWords()
    {
        // Arrange
        var testFile = Path.Combine(_testFilesDirectory, "test_short.txt");
        File.WriteAllText(testFile, "a\nab\nabc\nabcd");
        
        // Act
        var result = DictionarySanitizer.SanitizeDictionary(testFile, DictionaryFormat.OneWordPerLine);

        // Assert: Only words with 2-10 characters should be included
        Assert.Equal(3, result.SanitizedCount);
        Assert.DoesNotContain("A\ta", result.Lines);  // Too short (1 char)
        Assert.Contains("AB\tab", result.Lines);
        Assert.Contains("ABC\tabc", result.Lines);
        Assert.Contains("ABCD\tabcd", result.Lines);
        
        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SanitizeDictionary_ShouldFilterOutTooLongWords()
    {
        // Arrange
        var testFile = Path.Combine(_testFilesDirectory, "test_long.txt");
        File.WriteAllText(testFile, "short\nthisisverylongword");  // second word is 18 characters
        
        // Act
        var result = DictionarySanitizer.SanitizeDictionary(testFile, DictionaryFormat.OneWordPerLine);

        // Assert: Only words with 2-10 characters should be included
        Assert.Single(result.Lines);
        Assert.Contains("SHORT\tshort", result.Lines);
        Assert.DoesNotContain("THISISVERYLONGWORD", result.Lines);
        
        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SanitizeDictionary_ShouldFilterOutWordsWithNumbers()
    {
        // Arrange
        var testFile = Path.Combine(_testFilesDirectory, "test_numbers.txt");
        File.WriteAllText(testFile, "hello\nhel123lo\nworld9");
        
        // Act
        var result = DictionarySanitizer.SanitizeDictionary(testFile, DictionaryFormat.OneWordPerLine);

        // Assert: Only words with A-Z letters should be included
        Assert.Equal(1, result.SanitizedCount);
        Assert.Contains("HELLO\thello", result.Lines);
        
        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SanitizeDictionary_ShouldRemoveDuplicates()
    {
        // Arrange
        var testFile = Path.Combine(_testFilesDirectory, "test_dupes.txt");
        File.WriteAllText(testFile, "hello\nhello\nHELLO\nworld");
        
        // Act
        var result = DictionarySanitizer.SanitizeDictionary(testFile, DictionaryFormat.OneWordPerLine);

        // Assert
        Assert.Equal(2, result.SanitizedCount);
        Assert.Single(result.Lines, l => l.StartsWith("HELLO\t"));
        Assert.Contains("WORLD\tworld", result.Lines);
        
        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SanitizeDictionary_WithEmptyFile_ShouldReturnEmpty()
    {
        // Arrange
        var testFile = Path.Combine(_testFilesDirectory, "test_empty.txt");
        File.WriteAllText(testFile, "");
        
        // Act
        var result = DictionarySanitizer.SanitizeDictionary(testFile, DictionaryFormat.OneWordPerLine);

        // Assert
        Assert.Equal(0, result.SanitizedCount);
        Assert.Empty(result.Lines);
        
        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SanitizeDictionary_WithOnlyInvalidWords_ShouldReturnEmpty()
    {
        // Arrange
        var testFile = Path.Combine(_testFilesDirectory, "test_invalid.txt");
        File.WriteAllText(testFile, "a\n123\n***");
        
        // Act
        var result = DictionarySanitizer.SanitizeDictionary(testFile, DictionaryFormat.OneWordPerLine);

        // Assert
        Assert.Equal(0, result.SanitizedCount);
        Assert.Empty(result.Lines);
        
        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SanitizeDictionary_ShouldPreserveDiacriticsInOriginal()
    {
        // Arrange
        var testFile = Path.Combine(_testFilesDirectory, "test_diacritics.txt");
        File.WriteAllText(testFile, "café\naangeërfd");
        
        // Act
        var result = DictionarySanitizer.SanitizeDictionary(testFile, DictionaryFormat.OneWordPerLine);

        // Assert
        Assert.Equal(2, result.SanitizedCount);
        // Normalized form should not have diacritics, but original should preserve them
        Assert.Contains("CAFE\tcafé", result.Lines);
        Assert.Contains("AANGEERFD\taangeërfd", result.Lines);
        
        // Cleanup
        File.Delete(testFile);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(DictionaryFormat.TabSeparated, "word\t1000", "WORD\tword")]
    [InlineData(DictionaryFormat.SpaceSeparated, "word 1000", "WORD\tword")]
    [InlineData(DictionaryFormat.OneWordPerLine, "word", "WORD\tword")]
    public void SanitizeDictionary_WithDifferentFormats_ShouldProduceConsistentOutput(
        DictionaryFormat format, string inputText, string expectedLine)
    {
        // Arrange
        var testFile = Path.Combine(_testFilesDirectory, $"test_{format}.txt");
        File.WriteAllText(testFile, inputText);
        
        // Act
        var result = DictionarySanitizer.SanitizeDictionary(testFile, format);

        // Assert
        Assert.Single(result.Lines);
        Assert.Contains(expectedLine, result.Lines);
        
        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SanitizeAndSave_ShouldCreateOutputFile()
    {
        // Arrange
        var inputFile = Path.Combine(_testFilesDirectory, "input.txt");
        var outputFile = Path.Combine(_testFilesDirectory, "output.txt");
        File.WriteAllText(inputFile, "hello\nworld\ncafé");
        
        // Act
        DictionarySanitizer.SanitizeAndSave(inputFile, outputFile, DictionaryFormat.OneWordPerLine);

        // Assert
        Assert.True(File.Exists(outputFile));
        var lines = File.ReadAllLines(outputFile);
        Assert.Equal(3, lines.Length);
        Assert.Contains("CAFE\tcafé", lines);
        Assert.Contains("HELLO\thello", lines);
        Assert.Contains("WORLD\tworld", lines);
        
        // Cleanup
        File.Delete(inputFile);
        File.Delete(outputFile);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SanitizeDictionary_ShouldSortResults()
    {
        // Arrange
        var testFile = Path.Combine(_testFilesDirectory, "test_sort.txt");
        File.WriteAllText(testFile, "zebra\napple\nmango");
        
        // Act
        var result = DictionarySanitizer.SanitizeDictionary(testFile, DictionaryFormat.OneWordPerLine);

        // Assert: Results should be sorted alphabetically
        Assert.Equal("APPLE\tapple", result.Lines[0]);
        Assert.Equal("MANGO\tmango", result.Lines[1]);
        Assert.Equal("ZEBRA\tzebra", result.Lines[2]);
        
        // Cleanup
        File.Delete(testFile);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SanitizeDictionary_ShouldPreferLowercaseOverUppercase()
    {
        // Arrange
        var testFile = Path.Combine(_testFilesDirectory, "test_case.txt");
        File.WriteAllText(testFile, "HELLO\nhello");
        
        // Act
        var result = DictionarySanitizer.SanitizeDictionary(testFile, DictionaryFormat.OneWordPerLine);

        // Assert: Should prefer lowercase "hello" over "HELLO"
        Assert.Single(result.Lines);
        Assert.Contains("HELLO\thello", result.Lines);
        
        // Cleanup
        File.Delete(testFile);
    }
}
