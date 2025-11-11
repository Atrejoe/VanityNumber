namespace VanityNumberApi.Models;

/// <summary>
/// Information about an available dictionary.
/// </summary>
public class DictionaryInfo
{
    /// <summary>
    /// The name of the dictionary.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// The dictionary type value.
    /// </summary>
    public required int Value { get; init; }
    
    /// <summary>
    /// A description of what the dictionary contains.
    /// </summary>
    public required string Description { get; init; }
    
    /// <summary>
    /// Approximate number of words in the dictionary.
    /// </summary>
    public required int WordCount { get; init; }
}
