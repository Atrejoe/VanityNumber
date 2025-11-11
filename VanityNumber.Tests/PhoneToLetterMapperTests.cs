using VanityNumber.Core.Services;

namespace VanityNumber.Tests;


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
    [InlineData('4', new[] { 'G', 'H', 'I' })]
    [InlineData('5', new[] { 'J', 'K', 'L' })]
    [InlineData('6', new[] { 'M', 'N', 'O' })]
    [InlineData('7', new[] { 'P', 'Q', 'R', 'S' })]
    [InlineData('8', new[] { 'T', 'U', 'V' })]
    [InlineData('9', new[] { 'W', 'X', 'Y', 'Z' })]
    public void GetLettersForDigit_WithoutLeetSpeak_ShouldReturnStandardLetters(char digit, char[] expectedLetters)
    {
        // Act
        var result = _mapper.GetLettersForDigit(digit, useLeetSpeak: false);

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedLetters, result[0]);
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
    public void GetLettersForDigit_WithLeetSpeak_ShouldReturnCorrectLetters(char digit, char[] expectedLetters)
    {
        // Act
        var result = _mapper.GetLettersForDigit(digit, useLeetSpeak: true);

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedLetters, result[0]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void GetLettersForDigit_ForDigit0_WithLeetSpeak_ShouldReturnO()
    {
        // Act
        var result = _mapper.GetLettersForDigit('0', useLeetSpeak: true);

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(new[] { 'O' }, result[0]); // Leet: 0 = O
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void GetLettersForDigit_ForDigit1_WithLeetSpeak_ShouldReturnIL()
    {
        // Act
        var result = _mapper.GetLettersForDigit('1', useLeetSpeak: true);

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
        var result = _mapper.GenerateLetterCombinations(digits, useLeetSpeak: false);

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
        var result = _mapper.GenerateLetterCombinations("", useLeetSpeak: false);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void GenerateLetterCombinations_ForSingleDigit_ShouldReturnCorrectCount()
    {
        // Act
        var result = _mapper.GenerateLetterCombinations("2", useLeetSpeak: false);

        // Assert
        Assert.Equal(3, result.Length); // A, B, C
        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void GenerateLetterCombinations_ForBatman_WithLeetSpeak_ShouldIncludeLeetSpeakMapping()
    {
        // Arrange: BATMAN = B(8-leet) A(2) T(8) M(6) A(2) N(6)
        var digits = "828626";

        // Act
        var result = _mapper.GenerateLetterCombinations(digits, useLeetSpeak: true);

        // Assert: 8→[T,U,V,B] 2→[A,B,C] 8→[T,U,V,B] 6→[M,N,O] 2→[A,B,C] 6→[M,N,O]
        // Total combinations: 4 * 3 * 4 * 3 * 3 * 3 = 1296
        Assert.Equal(1296, result.Length);
        
        // Verify BATMAN is generated using leet speak (8=B)
        Assert.Contains("BATMAN", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    public void GenerateLetterCombinationsWithTracking_ForBatman_WithLeetSpeak_ShouldTrackLeetSpeak()
    {
        // Arrange: 828646 = B(8-leet) A(2) T(8) M(6) A(4-leet) N(6)
        var digits = "828646";

        // Act
        var result = _mapper.GenerateLetterCombinationsWithTracking(digits, useLeetSpeak: true);

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
        var combinations = _mapper.GenerateLetterCombinationsWithTracking("828646", useLeetSpeak: true);
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
        var combinations = _mapper.GenerateLetterCombinationsWithTracking("2468", useLeetSpeak: false);
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
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(digits, useLeetSpeak: true);
        var batman = combinations.FirstOrDefault(c => c.Letters == "BATMAN");
        
        Assert.NotNull(batman);
        var vanityDisplay = batman.ToVanityDisplay(); // "8atm4n"
        
        // Act: Convert vanity display back to digits using Core method
        var reconstructed = _mapper.ConvertVanityToDigits(vanityDisplay);
        
        // Assert: Should reconstruct to original digits
        Assert.Equal(digits, reconstructed);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [InlineData("0", "O", "0", true)]           // 0→O (leet)
    [InlineData("1", "I", "1", true)]           // 1→I (leet)
    [InlineData("1", "L", "1", true)]           // 1→L (leet)
    [InlineData("3", "E", "e", false)]          // 3→E (standard, NOT leet!)
    [InlineData("4", "A", "4", true)]           // 4→A (leet)
    [InlineData("5", "S", "5", true)]           // 5→S (leet)
    [InlineData("7", "T", "7", true)]           // 7→T (leet)
    [InlineData("8", "B", "8", true)]           // 8→B (leet)
    [InlineData("2", "A", "a", false)]          // 2→A (standard, lowercase)
    [InlineData("8", "T", "t", false)]          // 8→T (standard, lowercase)
    public void ToVanityDisplay_ForSingleDigit_ShouldShowCorrectFormat(
        string digit, string letter, string expectedDisplay, bool useLeetSpeak)
    {
        // Act
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(digit, useLeetSpeak);
        var match = combinations.FirstOrDefault(c => c.Letters == letter);
        
        // Assert
        Assert.NotNull(match);
        Assert.Equal(expectedDisplay, match.ToVanityDisplay());
    }

    #region Vanity Number Reversibility Tests

    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [Trait("Feature", "Reversibility")]
    [InlineData("2665", "COOL", false)]      // Standard mapping: 2=C, 6=O, 6=O, 5=L
    [InlineData("4663", "GOOD", false)]      // Standard mapping: 4=G, 6=O, 6=O, 3=D
    [InlineData("2255", "CALL", false)]      // Standard mapping: 2=C, 2=A, 5=L, 5=L
    [InlineData("3663", "FOOD", false)]      // Standard mapping: 3=F, 6=O, 6=O, 3=D
    [InlineData("7464", "RING", false)]      // Standard mapping: 7=R, 4=I, 6=N, 4=G
    public void AllVanityCombinations_WhenConvertedBack_ShouldMatchOriginalDigits(string originalDigits, string targetWord, bool useLeetSpeak)
    {
        // Arrange
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(originalDigits, useLeetSpeak);
        var targetCombo = combinations.FirstOrDefault(c => c.Letters == targetWord);
        
        Assert.NotNull(targetCombo);
        
        // Act: Get vanity display and convert back to digits using Core method
        var vanityDisplay = targetCombo.ToVanityDisplay();
        var reconstructed = _mapper.ConvertVanityToDigits(vanityDisplay);
        
        // Assert: Should always reconstruct to original
        Assert.Equal(originalDigits, reconstructed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [Trait("Feature", "Reversibility")]
    public void AllVanityCombinations_ForEveryPossibleCombination_ShouldMapBackToOriginal()
    {
        // Arrange: Test with a representative phone number
        var originalDigits = "2468";
        
        // Act: Generate all combinations
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(originalDigits, useLeetSpeak: false);
        
        // Assert: Every single combination should map back to the original digits
        foreach (var combo in combinations)
        {
            var vanityDisplay = combo.ToVanityDisplay();
            var reconstructed = _mapper.ConvertVanityToDigits(vanityDisplay);
            
            Assert.Equal(originalDigits, reconstructed);
        }
    }

    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [Trait("Feature", "Reversibility")]
    [InlineData("828646", true)]    // BATMAN with leet speak
    [InlineData("4663", false)]      // GOOD
    [InlineData("7464663", false)]   // RINGING
    [InlineData("22556", false)]     // CALL + O
    public void AllVanityCombinations_ForVariousNumbers_AllShouldReverseCorrectly(string originalDigits, bool useLeetSpeak)
    {
        // Arrange
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(originalDigits, useLeetSpeak);
        
        // Act & Assert: Every combination must reverse to original
        var failures = new List<string>();
        
        foreach (var combo in combinations)
        {
            var vanityDisplay = combo.ToVanityDisplay();
            var reconstructed = _mapper.ConvertVanityToDigits(vanityDisplay);
            
            if (reconstructed != originalDigits)
            {
                failures.Add($"Combination '{combo.Letters}' with display '{vanityDisplay}' reversed to '{reconstructed}' instead of '{originalDigits}'");
            }
        }
        
        // Assert: No failures
        Assert.Empty(failures);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [Trait("Feature", "Reversibility")]
    public void VanityNumber_WithMixedLeetAndStandard_ShouldPreserveOriginalDigits()
    {
        // Arrange: Complex case with both leet speak and standard mappings
        // 828646 = "8atm4n" (BATMAN with leet at positions 0 and 4)
        var originalDigits = "828646";
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(originalDigits, useLeetSpeak: true);
        var batman = combinations.First(c => c.Letters == "BATMAN");
        
        // Act
        var vanityDisplay = batman.ToVanityDisplay(); // Should be "8atm4n"
        var reconstructed = _mapper.ConvertVanityToDigits(vanityDisplay);
        
        // Assert
        Assert.Equal("8atm4n", vanityDisplay);
        Assert.Equal(originalDigits, reconstructed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [Trait("Feature", "Reversibility")]
    [Trait("Feature", "LeetSpeak")]
    public void VanityNumber_227646_ToLeetSpeakBa7m4n_AndBackToDigits()
    {
        // Arrange: 227646 with leet speak should produce "ba7m4n" (BATMAN)
        // 2=B, 2=A, 7=T(leet), 6=M, 4=A(leet), 6=N
        var originalDigits = "227646";
        
        // Act: Generate combinations with leet speak enabled
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(originalDigits, useLeetSpeak: true);
        var batman = combinations.FirstOrDefault(c => c.Letters == "BATMAN");
        
        // Assert: BATMAN should be found
        Assert.NotNull(batman);
        
        // Get vanity display - should show "ba7m4n"
        var vanityDisplay = batman.ToVanityDisplay();
        Assert.Equal("ba7m4n", vanityDisplay);
        
        // Verify leet speak usage
        Assert.False(batman.UsedLeetSpeak[0]);  // Position 0: 2→B (standard)
        Assert.False(batman.UsedLeetSpeak[1]);  // Position 1: 2→A (standard)
        Assert.True(batman.UsedLeetSpeak[2]);   // Position 2: 7→T (leet)
        Assert.False(batman.UsedLeetSpeak[3]);  // Position 3: 6→M (standard)
        Assert.True(batman.UsedLeetSpeak[4]);   // Position 4: 4→A (leet)
        Assert.False(batman.UsedLeetSpeak[5]);  // Position 5: 6→N (standard)
        
        // Convert back to digits
        var reconstructed = _mapper.ConvertVanityToDigits(vanityDisplay);
        Assert.Equal(originalDigits, reconstructed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [Trait("Feature", "Reversibility")]
    public void VanityNumber_WithOnlyLeetSpeak_ShouldShowOnlyDigits()
    {
        // Arrange: 01 = "OI" or "OL" (all leet speak)
        var originalDigits = "01";
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(originalDigits, useLeetSpeak: true);
        
        // Act & Assert: All combinations should be all digits (all leet)
        foreach (var combo in combinations)
        {
            var vanityDisplay = combo.ToVanityDisplay();
            
            // Should be all digits since all are leet speak
            Assert.Equal("01", vanityDisplay);
            Assert.All(vanityDisplay, c => Assert.True(char.IsDigit(c)));
            
            // Should reverse perfectly
            var reconstructed = _mapper.ConvertVanityToDigits(vanityDisplay);
            Assert.Equal(originalDigits, reconstructed);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [Trait("Feature", "Reversibility")]
    public void VanityNumber_WithNoLeetSpeak_ShouldShowOnlyLetters()
    {
        // Arrange: 23 = combinations like "AD", "AE", "AF", etc. (no leet speak used)
        var originalDigits = "23";
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(originalDigits, useLeetSpeak: false);
        
        // Act & Assert
        foreach (var combo in combinations)
        {
            // Check if this combination uses any leet speak
            var usesLeet = combo.UsedLeetSpeak.Any(x => x);
            
            if (!usesLeet)
            {
                var vanityDisplay = combo.ToVanityDisplay();
                
                // Should be all letters (lowercase)
                Assert.All(vanityDisplay, c => Assert.True(char.IsLetter(c) && char.IsLower(c)));
                
                // Should still reverse correctly
                var reconstructed = _mapper.ConvertVanityToDigits(vanityDisplay);
                Assert.Equal(originalDigits, reconstructed);
            }
        }
    }

    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [Trait("Feature", "Reversibility")]
    [InlineData("5678", false)]      // JOSH, JORT, etc.
    [InlineData("4663", false)]      // GOOD, HOME, etc.
    [InlineData("7464", false)]      // RING, PING, etc.
    [InlineData("2665", false)]      // BOOK, COOL, etc.
    public void VanityNumber_RealWorldPhoneNumbers_AllCombinationsShouldReverse(string phoneNumber, bool useLeetSpeak)
    {
        // Arrange
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(phoneNumber, useLeetSpeak);
        
        // Act & Assert: Test a sample of combinations
        var tested = 0;
        var maxToTest = Math.Min(100, combinations.Length); // Test up to 100 combinations
        
        foreach (var combo in combinations.Take(maxToTest))
        {
            var vanityDisplay = combo.ToVanityDisplay();
            var reconstructed = _mapper.ConvertVanityToDigits(vanityDisplay);
            
            Assert.Equal(phoneNumber, reconstructed);
            tested++;
        }
        
        // Ensure we actually tested some combinations
        Assert.True(tested > 0, $"Expected to test at least one combination for {phoneNumber}");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [Trait("Feature", "Reversibility")]
    public void VanityNumber_LongerPhoneNumber_ShouldMaintainReversibility()
    {
        // Arrange: 10-digit phone number (max length)
        var originalDigits = "5551234567";
        var combinations = _mapper.GenerateLetterCombinationsWithTracking(originalDigits, useLeetSpeak: false);
        
        // Act & Assert: Test a sample since there will be many combinations
        var tested = 0;
        var maxToTest = 50; // Sample 50 combinations
        
        foreach (var combo in combinations.Take(maxToTest))
        {
            var vanityDisplay = combo.ToVanityDisplay();
            var reconstructed = _mapper.ConvertVanityToDigits(vanityDisplay);
            
            Assert.Equal(originalDigits, reconstructed);
            Assert.Equal(originalDigits.Length, vanityDisplay.Length);
            tested++;
        }
        
        Assert.Equal(maxToTest, tested);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "PhoneToLetterMapper")]
    [Trait("Feature", "Reversibility")]
    public void ConvertVanityToDigits_WithVariousFormats_ShouldConvertCorrectly()
    {
        // Act & Assert: Test various vanity number formats
        Assert.Equal("2665", _mapper.ConvertVanityToDigits("cool"));
        Assert.Equal("2665", _mapper.ConvertVanityToDigits("COOL"));
        Assert.Equal("2665", _mapper.ConvertVanityToDigits("CoOl"));
        Assert.Equal("828646", _mapper.ConvertVanityToDigits("8atm4n"));
        Assert.Equal("227646", _mapper.ConvertVanityToDigits("ba7m4n"));
        Assert.Equal("4663", _mapper.ConvertVanityToDigits("good"));
        Assert.Equal("123", _mapper.ConvertVanityToDigits("123")); // All digits
    }

    #endregion
}
