using VanityNumberApi.Core.Services;
using Xunit;

namespace VanityNumberApi.Tests;

/// <summary>
/// Tests for the PhoneToLetterMapper service that converts phone digits to letters.
/// </summary>
public class PhoneToLetterMapperTests
{
    private readonly IPhoneToLetterMapper _mapper;

    public PhoneToLetterMapperTests()
    {
        _mapper = new PhoneToLetterMapper();
    }

    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [InlineData('2', new[] { 'A', 'B', 'C' })]
    [InlineData('3', new[] { 'D', 'E', 'F' })]
    [InlineData('4', new[] { 'G', 'H', 'I', 'A' })]        // Includes leet: 4=A
    [InlineData('5', new[] { 'J', 'K', 'L', 'S' })]        // Includes leet: 5=S
    [InlineData('6', new[] { 'M', 'N', 'O' })]
    [InlineData('7', new[] { 'P', 'Q', 'R', 'S', 'T' })]   // Includes leet: 7=T
    [InlineData('8', new[] { 'T', 'U', 'V', 'B' })]        // Includes leet: 8=B
    [InlineData('9', new[] { 'W', 'X', 'Y', 'Z' })]
    public void GetLettersForDigit_ShouldReturnCorrectLetters(char digit, char[] expectedLetters)
    {
        // Act
        var result = _mapper.GetLettersForDigit(digit);

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedLetters, result[0]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void GetLettersForDigit_ForDigit0_ShouldReturnO()
    {
        // Act
        var result = _mapper.GetLettersForDigit('0');

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(new[] { 'O' }, result[0]); // Leet: 0 = O
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void GetLettersForDigit_ForDigit1_ShouldReturnIL()
    {
        // Act
        var result = _mapper.GetLettersForDigit('1');

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(new[] { 'I', 'L' }, result[0]); // Leet: 1 = I or L
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void GenerateLetterCombinations_ForSimpleInput_ShouldReturnAllCombinations()
    {
        // Arrange
        var digits = "23";

        // Act
        var result = _mapper.GenerateLetterCombinations(digits);

        // Assert
        Assert.Equal(9, result.Length); // 3 letters * 3 letters = 9 combinations
        Assert.Contains("AD", result);
        Assert.Contains("AE", result);
        Assert.Contains("AF", result);
        Assert.Contains("BD", result);
        Assert.Contains("BE", result);
        Assert.Contains("BF", result);
        Assert.Contains("CD", result);
        Assert.Contains("CE", result);
        Assert.Contains("CF", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void GenerateLetterCombinations_ForEmptyString_ShouldReturnEmpty()
    {
        // Act
        var result = _mapper.GenerateLetterCombinations("");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void GenerateLetterCombinations_ForSingleDigit_ShouldReturnCorrectCount()
    {
        // Act
        var result = _mapper.GenerateLetterCombinations("2");

        // Assert
        Assert.Equal(3, result.Length); // A, B, C
        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }
}
