namespace VanityNumber.Core.Services;

/// <summary>
/// Represents a letter combination with information about which positions used leet speak.
/// </summary>
public class LetterCombination
{
    /// <summary>The generated letter combination (e.g., "BATMAN").</summary>
    public string Letters { get; set; } = string.Empty;
    
    /// <summary>Boolean array indicating which positions used leet speak mappings.</summary>
    public bool[] UsedLeetSpeak { get; set; } = Array.Empty<bool>();
    
    /// <summary>The original digits that generated this combination.</summary>
    public string OriginalDigits { get; set; } = string.Empty;
    
    /// <summary>
    /// Converts the combination to a vanity display format.
    /// Shows original digit where leet speak was used, letter otherwise.
    /// Example: "828646" + "BATMAN" with leet at positions 0,4 = "8atm4n"
    /// </summary>
    public string ToVanityDisplay()
    {
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < Letters.Length; i++)
        {
            if (UsedLeetSpeak[i])
            {
                // Show original digit when leet speak was used
                result.Append(OriginalDigits[i]);
            }
            else
            {
                // Show lowercase letter when standard mapping was used
                result.Append(char.ToLowerInvariant(Letters[i]));
            }
        }
        return result.ToString();
    }
}

/// <summary>
/// Interface for mapping phone digits to letters using standard T9 keypad layout.
/// </summary>
public interface IPhoneToLetterMapper
{
    /// <summary>
    /// Gets the letters associated with a specific digit on the phone keypad.
    /// </summary>
    /// <param name="digit">The digit character (0-9).</param>
    /// <param name="useLeetSpeak">Whether to include leet speak mappings (default: false).</param>
    /// <returns>An array containing the letters for the digit, or empty if not found.</returns>
    char[][] GetLettersForDigit(char digit, bool useLeetSpeak = false);
    
    /// <summary>
    /// Generates all possible letter combinations for a sequence of digits.
    /// </summary>
    /// <param name="digits">The string of digits to convert.</param>
    /// <param name="useLeetSpeak">Whether to include leet speak mappings (default: false).</param>
    /// <returns>An array of all possible letter combinations.</returns>
    string[] GenerateLetterCombinations(string digits, bool useLeetSpeak = false);
    
    /// <summary>
    /// Generates all possible letter combinations with leet speak tracking.
    /// </summary>
    /// <param name="digits">The string of digits to convert.</param>
    /// <param name="useLeetSpeak">Whether to include leet speak mappings (default: false).</param>
    /// <returns>An array of letter combinations with leet speak position tracking.</returns>
    LetterCombination[] GenerateLetterCombinationsWithTracking(string digits, bool useLeetSpeak = false);
    
    /// <summary>
    /// Converts a vanity display string back to its original digits.
    /// Handles both lowercase letters and digits (from leet speak).
    /// </summary>
    /// <param name="vanityDisplay">The vanity display string (e.g., "8atm4n" or "cool").</param>
    /// <returns>The original digit sequence.</returns>
    string ConvertVanityToDigits(string vanityDisplay);
}

/// <summary>
/// Maps phone digits to letters using the standard T9 telephone keypad layout.
/// Supports optional leet speak mappings: 0=O, 1=I/L, 3=E, 4=A, 5=S, 7=T, 8=B.
/// </summary>
public class PhoneToLetterMapper : IPhoneToLetterMapper
{
    // Standard T9 phone keypad mapping (without leet speak)
    private readonly Dictionary<char, char[]> _standardDigitToLetters = new()
    {
        { '2', new[] { 'A', 'B', 'C' } },                 // Standard T9
        { '3', new[] { 'D', 'E', 'F' } },                 // Standard T9
        { '4', new[] { 'G', 'H', 'I' } },                 // Standard T9
        { '5', new[] { 'J', 'K', 'L' } },                 // Standard T9
        { '6', new[] { 'M', 'N', 'O' } },                 // Standard T9
        { '7', new[] { 'P', 'Q', 'R', 'S' } },            // Standard T9
        { '8', new[] { 'T', 'U', 'V' } },                 // Standard T9
        { '9', new[] { 'W', 'X', 'Y', 'Z' } }             // Standard T9
    };

    // Enhanced phone keypad mapping with leet speak support
    // Standard T9 + leet speak alternatives: 0=O, 1=I/L, 3=E, 4=A, 5=S, 7=T, 8=B
    private readonly Dictionary<char, char[]> _digitToLettersWithLeet = new()
    {
        { '0', new[] { 'O' } },                           // Leet: 0 = O
        { '1', new[] { 'I', 'L' } },                      // Leet: 1 = I or L
        { '2', new[] { 'A', 'B', 'C' } },                 // Standard T9
        { '3', new[] { 'D', 'E', 'F' } },                 // Standard T9 (E also leet)
        { '4', new[] { 'G', 'H', 'I', 'A' } },            // Standard T9 + Leet: 4 = A
        { '5', new[] { 'J', 'K', 'L', 'S' } },            // Standard T9 + Leet: 5 = S
        { '6', new[] { 'M', 'N', 'O' } },                 // Standard T9
        { '7', new[] { 'P', 'Q', 'R', 'S', 'T' } },       // Standard T9 + Leet: 7 = T
        { '8', new[] { 'T', 'U', 'V', 'B' } },            // Standard T9 + Leet: 8 = B
        { '9', new[] { 'W', 'X', 'Y', 'Z' } }             // Standard T9
    };
    
    // Maps each digit to the letters that are considered "leet speak" for that digit
    private readonly Dictionary<char, HashSet<char>> _leetSpeakLetters = new()
    {
        { '0', new HashSet<char> { 'O' } },               // 0 = O (all leet)
        { '1', new HashSet<char> { 'I', 'L' } },          // 1 = I or L (all leet)
        { '2', new HashSet<char>() },                     // No leet speak
        { '3', new HashSet<char>() },                     // No leet speak (E is standard T9)
        { '4', new HashSet<char> { 'A' } },               // 4 = A (leet)
        { '5', new HashSet<char> { 'S' } },               // 5 = S (leet)
        { '6', new HashSet<char>() },                     // No leet speak
        { '7', new HashSet<char> { 'T' } },               // 7 = T (leet)
        { '8', new HashSet<char> { 'B' } },               // 8 = B (leet)
        { '9', new HashSet<char>() }                      // No leet speak
    };
    
    // Letter to digit mapping for reverse conversion
    private readonly Dictionary<char, char> _letterToDigit = new()
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

    /// <inheritdoc />
    public char[][] GetLettersForDigit(char digit, bool useLeetSpeak = false)
    {
        var mapping = useLeetSpeak ? _digitToLettersWithLeet : _standardDigitToLetters;
        
        if (mapping.TryGetValue(digit, out var letters))
        {
            return new[] { letters };
        }
        return Array.Empty<char[]>();
    }

    /// <inheritdoc />
    public string[] GenerateLetterCombinations(string digits, bool useLeetSpeak = false)
    {
        if (string.IsNullOrEmpty(digits))
            return Array.Empty<string>();

        var result = new List<string>();
        GenerateCombinationsRecursive(digits, 0, "", result, useLeetSpeak);
        return result.ToArray();
    }
    
    /// <inheritdoc />
    public LetterCombination[] GenerateLetterCombinationsWithTracking(string digits, bool useLeetSpeak = false)
    {
        if (string.IsNullOrEmpty(digits))
            return Array.Empty<LetterCombination>();

        var result = new List<LetterCombination>();
        GenerateCombinationsWithTrackingRecursive(digits, 0, "", new bool[digits.Length], result, useLeetSpeak);
        return result.ToArray();
    }
    
    /// <inheritdoc />
    public string ConvertVanityToDigits(string vanityDisplay)
    {
        if (string.IsNullOrEmpty(vanityDisplay))
            return string.Empty;
            
        return new string(vanityDisplay.Select(c => 
            char.IsDigit(c) ? c : _letterToDigit[char.ToLowerInvariant(c)]
        ).ToArray());
    }

    /// <summary>
    /// Recursively generates all letter combinations with leet speak tracking.
    /// </summary>
    /// <param name="digits">The string of digits to convert.</param>
    /// <param name="index">Current position in the digits string.</param>
    /// <param name="current">Current combination being built.</param>
    /// <param name="leetFlags">Boolean array tracking leet speak usage.</param>
    /// <param name="result">List to store all generated combinations.</param>
    /// <param name="useLeetSpeak">Whether to include leet speak mappings.</param>
    private void GenerateCombinationsWithTrackingRecursive(
        string digits, 
        int index, 
        string current, 
        bool[] leetFlags,
        List<LetterCombination> result,
        bool useLeetSpeak)
    {
        if (index == digits.Length)
        {
            result.Add(new LetterCombination
            {
                Letters = current,
                UsedLeetSpeak = (bool[])leetFlags.Clone(),
                OriginalDigits = digits
            });
            return;
        }

        char digit = digits[index];
        var mapping = useLeetSpeak ? _digitToLettersWithLeet : _standardDigitToLetters;
        
        if (mapping.TryGetValue(digit, out var letters))
        {
            foreach (var letter in letters)
            {
                // Check if this letter is a leet speak mapping for this digit
                bool isLeet = useLeetSpeak && 
                              _leetSpeakLetters.TryGetValue(digit, out var leetSet) && 
                              leetSet.Contains(letter);
                
                var newLeetFlags = (bool[])leetFlags.Clone();
                newLeetFlags[index] = isLeet;
                
                GenerateCombinationsWithTrackingRecursive(
                    digits, 
                    index + 1, 
                    current + letter, 
                    newLeetFlags,
                    result,
                    useLeetSpeak);
            }
        }
        else
        {
            // If digit not found, just add the digit itself
            var newLeetFlags = (bool[])leetFlags.Clone();
            newLeetFlags[index] = false;
            
            GenerateCombinationsWithTrackingRecursive(
                digits, 
                index + 1, 
                current + digit, 
                newLeetFlags,
                result,
                useLeetSpeak);
        }
    }

    /// <summary>
    /// Recursively generates all letter combinations for the given digits.
    /// </summary>
    /// <param name="digits">The string of digits to convert.</param>
    /// <param name="index">Current position in the digits string.</param>
    /// <param name="current">Current combination being built.</param>
    /// <param name="result">List to store all generated combinations.</param>
    /// <param name="useLeetSpeak">Whether to include leet speak mappings.</param>
    private void GenerateCombinationsRecursive(string digits, int index, string current, List<string> result, bool useLeetSpeak)
    {
        if (index == digits.Length)
        {
            result.Add(current);
            return;
        }

        var mapping = useLeetSpeak ? _digitToLettersWithLeet : _standardDigitToLetters;
        
        if (mapping.TryGetValue(digits[index], out var letters))
        {
            foreach (var letter in letters)
            {
                GenerateCombinationsRecursive(digits, index + 1, current + letter, result, useLeetSpeak);
            }
        }
        else
        {
            // If digit not found, just add the digit itself
            GenerateCombinationsRecursive(digits, index + 1, current + digits[index], result, useLeetSpeak);
        }
    }
}
