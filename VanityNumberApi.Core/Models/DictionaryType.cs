namespace VanityNumberApi.Core.Models;

/// <summary>
/// Represents the types of dictionaries available for vanity number generation.
/// This is a flags enumeration allowing multiple dictionaries to be combined using bitwise OR.
/// </summary>
[Flags]
public enum DictionaryType
{
    /// <summary>
    /// No dictionary selected.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Dutch language dictionary.
    /// </summary>
    Dutch = 1,
    
    /// <summary>
    /// English language dictionary.
    /// </summary>
    English = 2,
    
    /// <summary>
    /// Urban slang dictionary.
    /// </summary>
    Urban = 4,
    
    /// <summary>
    /// All available dictionaries (Dutch, English, and Urban).
    /// </summary>
    All = Dutch | English | Urban
}
