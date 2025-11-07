using System.ComponentModel.DataAnnotations;

namespace VanityNumberApi.Core.Models;

/// <summary>
/// Request model for converting a phone number to vanity numbers.
/// </summary>
public class VanityNumberRequest
{
    /// <summary>
    /// Gets or sets the phone number to convert.
    /// Can include formatting characters like spaces, dashes, or parentheses.
    /// After cleaning, must contain 3-10 digits.
    /// </summary>
    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "Phone number must be between 1 and 30 characters")]
    [RegularExpression(@"^[\d\s\-\(\)\+\.]+$", ErrorMessage = "Phone number can only contain digits, spaces, dashes, parentheses, plus signs, and dots")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the dictionary types to search.
    /// Multiple dictionaries can be combined using bitwise OR (e.g., Dutch | English).
    /// Defaults to All dictionaries.
    /// </summary>
    public DictionaryType DictionaryTypes { get; set; } = DictionaryType.All;
    
    /// <summary>
    /// Gets or sets the minimum word length to search for.
    /// Defaults to 3 characters.
    /// </summary>
    [Range(2, 15, ErrorMessage = "Minimum word length must be between 2 and 15")]
    public int? MinWordLength { get; set; } = 3;
    
    /// <summary>
    /// Gets or sets the maximum number of results to return.
    /// Defaults to 20 results.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Maximum results must be between 1 and 100")]
    public int? MaxResults { get; set; } = 20;
}

/// <summary>
/// Result model containing vanity number matches for a phone number.
/// </summary>
public class VanityNumberResult
{
    /// <summary>
    /// Gets or sets the original phone number from the request.
    /// </summary>
    public string OriginalNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the list of vanity number matches found.
    /// </summary>
    public List<VanityMatch> Matches { get; set; } = new();
}

/// <summary>
/// Represents a single vanity number match found in a phone number.
/// </summary>
public class VanityMatch
{
    /// <summary>
    /// Gets or sets the vanity number with the word replacing digits.
    /// </summary>
    public string VanityNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the word found in the dictionary.
    /// </summary>
    public string Word { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the dictionary type(s) where this word was found.
    /// May contain multiple dictionary types if the word exists in multiple dictionaries.
    /// </summary>
    public DictionaryType DictionaryType { get; set; }
    
    /// <summary>
    /// Gets or sets the starting position of the word in the phone number.
    /// </summary>
    public int StartPosition { get; set; }
    
    /// <summary>
    /// Gets or sets the length of the word in digits.
    /// </summary>
    public int Length { get; set; }
}
