using VanityNumberApi.Core.Services;

namespace VanityNumberApi.Tests;


#pragma warning disable CS1591 // Missing XML comment : method should be self explanatory (for now)

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

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void GenerateLetterCombinations_ForBatman_ShouldIncludeLeetSpeakMapping()
    {
        // Arrange: BATMAN = B(8-leet) A(2) T(8) M(6) A(2) N(6)
        var digits = "828626";

        // Act
        var result = _mapper.GenerateLetterCombinations(digits);

        // Assert: 8→[T,U,V,B] 2→[A,B,C] 8→[T,U,V,B] 6→[M,N,O] 2→[A,B,C] 6→[M,N,O]
        // Total combinations: 4 * 3 * 4 * 3 * 3 * 3 = 1296
        Assert.Equal(1296, result.Length);
        
        // Verify BATMAN is generated using leet speak (8=B)
        Assert.Contains("BATMAN", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void GenerateLetterCombinationsWithTracking_ForBatman_ShouldTrackLeetSpeak()
    {
        // Arrange: 828646 = B(8-leet) A(2) T(8) M(6) A(4-leet) N(6)
        var digits = "828646";

        // Act
        var result = _mapper.GenerateLetterCombinationsWithTracking(digits);

        // Assert: Find the BATMAN combination
        var batman = result.FirstOrDefault(c => c.Letters == "BATMAN");
        Assert.NotNull(batman);
        
        // Verify leet speak tracking
        Assert.True(batman.UsedLeetSpeak[0]);   // Position 0: 8→B (leet)
        Assert.False(batman.UsedLeetSpeak[1]);  // Position 1: 2→A (standard)
        Assert.False(batman.UsedLeetSpeak[2]);  // Position 2: 8→T (standard)
        Assert.False(batman.UsedLeetSpeak[3]);  // Position 3: 6→M (standard)
        Assert.True(batman.UsedLeetSpeak[4]);   // Position 4: 4→A (leet)
        Assert.False(batman.UsedLeetSpeak[5]);  // Position 5: 6→N (standard)
        
        // Verify vanity display: "8atm4n"
        Assert.Equal("8atm4n", batman.ToVanityDisplay());
        Assert.Equal("828646", batman.OriginalDigits);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void ToVanityDisplay_ForBatman_ShouldShowCorrectFormat()
    {
        // Arrange
        var combinations = _mapper.GenerateLetterCombinationsWithTracking("828646");
        var batman = combinations.FirstOrDefault(c => c.Letters == "BATMAN");
        
        // Assert
        Assert.NotNull(batman);
        Assert.Equal("8atm4n", batman.ToVanityDisplay());
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void ToVanityDisplay_ForStandardMapping_ShouldShowLowercase()
    {
        // Arrange: 2→A, 4→G, 6→M, 8→T (all standard mappings)
        var combinations = _mapper.GenerateLetterCombinationsWithTracking("2468");
        var agmt = combinations.FirstOrDefault(c => c.Letters == "AGMT");
        
        // Assert
        Assert.NotNull(agmt);
        Assert.Equal("agmt", agmt.ToVanityDisplay());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void ToVanityDisplay_WhenConvertedBackToDigits_ShouldPreserveOriginalNumber()
    {
        // Arrange
        var digits = "828646";
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(digits);
        var batman = combinations.FirstOrDefault(c => c.Letters == "BATMAN");
        
        Assert.NotNull(batman);
        var vanityDisplay = batman.ToVanityDisplay(); // "8atm4n"
        
        // Act: Convert vanity display back to digits
        var digitMapping = new Dictionary<char, char>
        {
            {'a', '2'}, {'b', '2'}, {'c', '2'},
            {'d', '3'}, {'e', '3'}, {'f', '3'},
            {'g', '4'}, {'h', '4'}, {'i', '4'},
            {'j', '5'}, {'k', '5'}, {'l', '5'},
            {'m', '6'}, {'n', '6'}, {'o', '6'},
            {'p', '7'}, {'q', '7'}, {'r', '7'}, {'s', '7'},
            {'t', '8'}, {'u', '8'}, {'v', '8'},
            {'w', '9'}, {'x', '9'}, {'y', '9'}, {'z', '9'}
        };
        
        var reconstructed = new string(vanityDisplay.Select(c => 
            char.IsDigit(c) ? c : digitMapping[char.ToLowerInvariant(c)]
        ).ToArray());
        
        // Assert: Should reconstruct to original digits
        Assert.Equal(digits, reconstructed);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [InlineData("0", "O", "0")]           // 0→O (leet)
    [InlineData("1", "I", "1")]           // 1→I (leet)
    [InlineData("1", "L", "1")]           // 1→L (leet)
    [InlineData("3", "E", "e")]           // 3→E (standard, NOT leet!)
    [InlineData("4", "A", "4")]           // 4→A (leet)
    [InlineData("5", "S", "5")]           // 5→S (leet)
    [InlineData("7", "T", "7")]           // 7→T (leet)
    [InlineData("8", "B", "8")]           // 8→B (leet)
    [InlineData("2", "A", "a")]           // 2→A (standard, lowercase)
    [InlineData("8", "T", "t")]           // 8→T (standard, lowercase)
    public void ToVanityDisplay_ForSingleDigit_ShouldShowCorrectFormat(
        string digit, string letter, string expectedDisplay)
    {
        // Act
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(digit);
        var match = combinations.FirstOrDefault(c => c.Letters == letter);
        
        // Assert
        Assert.NotNull(match);
        Assert.Equal(expectedDisplay, match.ToVanityDisplay());
    }
}
