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
}

/// <summary>
/// Provides word lookup and validation services using Dutch, English, and Urban dictionaries.
/// </summary>
public class DictionaryService : IDictionaryService
{
    private readonly HashSet<string> _dutchWords;
    private readonly HashSet<string> _englishWords;
    private readonly HashSet<string> _urbanWords;

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
    /// </summary>
    /// <param name="resourceName">Fully qualified name of the embedded resource.</param>
    /// <returns>A hash set containing all words from the dictionary.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the dictionary resource is not found.</exception>
    private static HashSet<string> LoadDictionary(string resourceName)
    {
        var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
                    words.Add(trimmed.ToUpperInvariant());
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

        if (dictionaryType.HasFlag(DictionaryType.Dutch) && _dutchWords.Contains(upperWord))
            return true;
        if (dictionaryType.HasFlag(DictionaryType.English) && _englishWords.Contains(upperWord))
            return true;
        if (dictionaryType.HasFlag(DictionaryType.Urban) && _urbanWords.Contains(upperWord))
            return true;

        return false;
    }

    /// <inheritdoc />
    public IEnumerable<string> FindWords(IEnumerable<string> candidates, DictionaryType dictionaryTypes)
    {
        if (dictionaryTypes == DictionaryType.None)
            return Enumerable.Empty<string>();

        return candidates.Where(c => IsWord(c, dictionaryTypes));
    }
}
