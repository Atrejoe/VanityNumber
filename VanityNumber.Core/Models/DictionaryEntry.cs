namespace VanityNumber.Core.Models;

/// <summary>
/// Represents a dictionary entry including its original word form and a short definition (may be empty).
/// </summary>
public sealed class DictionaryEntry
{
    /// <summary>Original word with casing/diacritics preserved.</summary>
    public string Original { get; }
    /// <summary>Short definition or description; empty if not available.</summary>
    public string Definition { get; }

    /// <summary>Creates a new dictionary entry.</summary>
    /// <param name="original">Original word.</param>
    /// <param name="definition">Definition text (may be empty).</param>
    public DictionaryEntry(string original, string definition)
    {
        Original = original;
        Definition = definition;
    }
}
