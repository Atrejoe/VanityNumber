namespace VanityNumberApi.Core.Services;

/// <summary>
/// Interface for mapping phone digits to letters using standard T9 keypad layout.
/// </summary>
public interface IPhoneToLetterMapper
{
    /// <summary>
    /// Gets the letters associated with a specific digit on the phone keypad.
    /// </summary>
    /// <param name="digit">The digit character (0-9).</param>
    /// <returns>An array containing the letters for the digit, or empty if not found.</returns>
    char[][] GetLettersForDigit(char digit);
    
    /// <summary>
    /// Generates all possible letter combinations for a sequence of digits.
    /// </summary>
    /// <param name="digits">The string of digits to convert.</param>
    /// <returns>An array of all possible letter combinations.</returns>
    string[] GenerateLetterCombinations(string digits);
}

/// <summary>
/// Maps phone digits to letters using the standard T9 telephone keypad layout.
/// Supports leet speak mappings: 0=O, 1=I/L, 3=E, 4=A, 5=S, 7=T, 8=B.
/// </summary>
public class PhoneToLetterMapper : IPhoneToLetterMapper
{
    // Enhanced phone keypad mapping with leet speak support
    // Standard T9 + leet speak alternatives: 0=O, 1=I/L, 3=E, 4=A, 5=S, 7=T, 8=B
    private readonly Dictionary<char, char[]> _digitToLetters = new()
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

    /// <inheritdoc />
    public char[][] GetLettersForDigit(char digit)
    {
        if (_digitToLetters.TryGetValue(digit, out var letters))
        {
            return new[] { letters };
        }
        return Array.Empty<char[]>();
    }

    /// <inheritdoc />
    public string[] GenerateLetterCombinations(string digits)
    {
        if (string.IsNullOrEmpty(digits))
            return Array.Empty<string>();

        var result = new List<string>();
        GenerateCombinationsRecursive(digits, 0, "", result);
        return result.ToArray();
    }

    /// <summary>
    /// Recursively generates all letter combinations for the given digits.
    /// </summary>
    /// <param name="digits">The string of digits to convert.</param>
    /// <param name="index">Current position in the digits string.</param>
    /// <param name="current">Current combination being built.</param>
    /// <param name="result">List to store all generated combinations.</param>
    private void GenerateCombinationsRecursive(string digits, int index, string current, List<string> result)
    {
        if (index == digits.Length)
        {
            result.Add(current);
            return;
        }

        if (_digitToLetters.TryGetValue(digits[index], out var letters))
        {
            foreach (var letter in letters)
            {
                GenerateCombinationsRecursive(digits, index + 1, current + letter, result);
            }
        }
        else
        {
            // If digit not found, just add the digit itself
            GenerateCombinationsRecursive(digits, index + 1, current + digits[index], result);
        }
    }
}
