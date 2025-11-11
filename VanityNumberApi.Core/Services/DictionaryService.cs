using VanityNumberApi.Core.Models;
using System.Reflection;

namespace VanityNumberApi.Core.Services;

/// <summary>
/// Interface for dictionary word lookup and validation services.
/// </summary>
public interface IDictionaryService
{
    /// <summary>
    /// Checks if a word exists in the specified dictionary type(s).
    /// </summary>
    /// <param name="word">The word to check.</param>
    /// <param name="dictionaryType">The dictionary type(s) to search. Can be combined using bitwise OR.</param>
    /// <returns>True if the word exists in any of the specified dictionaries; otherwise, false.</returns>
    bool IsWord(string word, DictionaryType dictionaryType);

    /// <summary>
    /// Finds all words from the candidate list that exist in the specified dictionary type(s).
    /// </summary>
    /// <param name="candidates">The list of candidate words to check.</param>
    /// <param name="dictionaryTypes">The dictionary type(s) to search. Can be combined using bitwise OR.</param>
    /// <returns>An enumerable of words that were found in the dictionaries.</returns>
    IEnumerable<string> FindWords(IEnumerable<string> candidates, DictionaryType dictionaryTypes);

    /// <summary>
    /// Gets the number of words in a specific dictionary.
    /// </summary>
    /// <param name="dictionaryType">The dictionary type (must be a single value, not combined).</param>
    /// <returns>The number of words in the specified dictionary, or 0 if the type is None or All.</returns>
    int GetWordCount(DictionaryType dictionaryType);
}

/// <summary>
/// Provides word lookup and validation services using Dutch, English, and Urban dictionaries.
/// </summary>
public class DictionaryService : IDictionaryService
{
    // Maps normalized word -> original word (with diacritics)
    private readonly Dictionary<string, string> _dutchWords;
    private readonly Dictionary<string, string> _englishWords;
    private readonly Dictionary<string, string> _urbanWords;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryService"/> class.
    /// Loads all dictionaries from embedded resources.
    /// </summary>
    /// <exception cref="FileNotFoundException">Thrown when a required dictionary resource is not found.</exception>
    public DictionaryService()
    {
        _dutchWords = LoadDictionary("VanityNumberApi.Core.Dictionaries.dutch.txt");
        _englishWords = LoadDictionary("VanityNumberApi.Core.Dictionaries.english.txt");
        _urbanWords = LoadDictionary("VanityNumberApi.Core.Dictionaries.urban.txt");
    }

    /// <summary>
    /// Loads a dictionary from an embedded resource.
    /// Format: Each line is "NORMALIZED[tab]ORIGINAL" where ORIGINAL preserves diacritics.
    /// </summary>
    /// <param name="resourceName">Fully qualified name of the embedded resource.</param>
    /// <returns>A dictionary mapping normalized words to their original forms.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the dictionary resource is not found.</exception>
    private static Dictionary<string, string> LoadDictionary(string resourceName)
    {
        var words = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Dictionary resource '{resourceName}' not found. Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");
        
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line != null)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    // Parse format: NORMALIZED[tab]ORIGINAL
                    var parts = trimmed.Split('\t');
                    var normalized = parts[0].ToUpperInvariant();
                    var original = parts.Length > 1 ? parts[1] : parts[0];
                    
                    words[normalized] = original;
                }
            }
        }

        return words;
    }

    /// <inheritdoc />
    public bool IsWord(string word, DictionaryType dictionaryType)
    {
        if (dictionaryType == DictionaryType.None)
            return false;

        var upperWord = word.ToUpperInvariant();

        if (dictionaryType.HasFlag(DictionaryType.Dutch) && _dutchWords.ContainsKey(upperWord))
            return true;
        if (dictionaryType.HasFlag(DictionaryType.English) && _englishWords.ContainsKey(upperWord))
            return true;
        if (dictionaryType.HasFlag(DictionaryType.Urban) && _urbanWords.ContainsKey(upperWord))
            return true;

        return false;
    }

    /// <inheritdoc />
    public IEnumerable<string> FindWords(IEnumerable<string> candidates, DictionaryType dictionaryTypes)
    {
        if (dictionaryTypes == DictionaryType.None)
            return Enumerable.Empty<string>();

        var results = new List<string>();
        
        foreach (var candidate in candidates)
        {
            var upperWord = candidate.ToUpperInvariant();
            string? originalForm = null;
            
            // Try to find original form from dictionaries in order of preference
            if (dictionaryTypes.HasFlag(DictionaryType.Dutch) && _dutchWords.TryGetValue(upperWord, out var dutchOriginal))
                originalForm = dutchOriginal;
            else if (dictionaryTypes.HasFlag(DictionaryType.English) && _englishWords.TryGetValue(upperWord, out var englishOriginal))
                originalForm = englishOriginal;
            else if (dictionaryTypes.HasFlag(DictionaryType.Urban) && _urbanWords.TryGetValue(upperWord, out var urbanOriginal))
                originalForm = urbanOriginal;
            
            if (originalForm != null)
                results.Add(originalForm);
        }
        
        return results;
    }

    /// <inheritdoc />
    public int GetWordCount(DictionaryType dictionaryType)
    {
        return dictionaryType switch
        {
            DictionaryType.Dutch => _dutchWords.Count,
            DictionaryType.English => _englishWords.Count,
            DictionaryType.Urban => _urbanWords.Count,
            _ => 0
        };
    }
}
