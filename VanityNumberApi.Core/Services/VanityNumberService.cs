using VanityNumberApi.Core.Models;

namespace VanityNumberApi.Core.Services;

/// <summary>
/// Interface for vanity number generation services.
/// </summary>
public interface IVanityNumberService
{
    /// <summary>
    /// Generates vanity numbers from a phone number by finding dictionary words.
    /// </summary>
    /// <param name="request">The vanity number generation request containing phone number and search parameters.</param>
    /// <returns>A result containing all vanity number matches found.</returns>
    VanityNumberResult GenerateVanityNumbers(VanityNumberRequest request);
}

/// <summary>
/// Service for generating vanity numbers by finding dictionary words in phone number digit sequences.
/// </summary>
public class VanityNumberService : IVanityNumberService
{
    private readonly IPhoneToLetterMapper _letterMapper;
    private readonly IDictionaryService _dictionaryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="VanityNumberService"/> class.
    /// </summary>
    /// <param name="letterMapper">The phone to letter mapping service.</param>
    /// <param name="dictionaryService">The dictionary lookup service.</param>
    public VanityNumberService(IPhoneToLetterMapper letterMapper, IDictionaryService dictionaryService)
    {
        _letterMapper = letterMapper;
        _dictionaryService = dictionaryService;
    }

    /// <inheritdoc />
    public VanityNumberResult GenerateVanityNumbers(VanityNumberRequest request)
    {
        var result = new VanityNumberResult
        {
            OriginalNumber = request.PhoneNumber
        };

        // Clean the phone number (remove spaces, dashes, etc.)
        var cleanNumber = new string(request.PhoneNumber.Where(char.IsDigit).ToArray());
        
        if (string.IsNullOrEmpty(cleanNumber))
        {
            return result;
        }

        // Validate phone number length (max 10 digits for phone keypad mapping)
        if (cleanNumber.Length < 3)
        {
            throw new ArgumentException("Phone number must contain at least 3 digits after cleaning.", nameof(request.PhoneNumber));
        }
        
        if (cleanNumber.Length > 10)
        {
            throw new ArgumentException("Phone number must contain at most 10 digits after cleaning.", nameof(request.PhoneNumber));
        }

        var dictionaryTypes = request.DictionaryTypes;
        var minWordLength = request.MinWordLength ?? 3;
        var maxResults = request.MaxResults ?? 20;

        // Find vanity matches
        var matches = new List<VanityMatch>();

        // Try to find words in different segments of the phone number
        for (int start = 0; start < cleanNumber.Length; start++)
        {
            for (int length = minWordLength; length <= cleanNumber.Length - start && length <= 10; length++)
            {
                var segment = cleanNumber.Substring(start, length);
                var combinations = _letterMapper.GenerateLetterCombinations(segment);

                var foundWords = _dictionaryService.FindWords(combinations, dictionaryTypes);
                
                foreach (var word in foundWords)
                {
                    // Determine which dictionary(ies) contain this word
                    var matchedDictionaries = GetMatchedDictionaries(word, dictionaryTypes);
                    
                    var vanityNumber = BuildVanityNumber(cleanNumber, start, length, word);
                    
                    matches.Add(new VanityMatch
                    {
                        VanityNumber = vanityNumber,
                        Word = word,
                        DictionaryType = matchedDictionaries,
                        StartPosition = start,
                        Length = length
                    });

                    if (matches.Count >= maxResults * 3) // Get more than needed, we'll filter later
                    {
                        break;
                    }
                }
            }
        }

        // Sort by word length (longer words first) and take the requested number
        result.Matches = matches
            .OrderByDescending(m => m.Length)
            .ThenBy(m => m.StartPosition)
            .Take(maxResults)
            .ToList();

        return result;
    }

    /// <summary>
    /// Builds a vanity number string by replacing a segment of digits with a word.
    /// </summary>
    /// <param name="phoneNumber">The original phone number.</param>
    /// <param name="start">The starting position of the segment to replace.</param>
    /// <param name="length">The length of the segment to replace.</param>
    /// <param name="word">The word to insert.</param>
    /// <returns>The vanity number with the word replacing the digit segment.</returns>
    private string BuildVanityNumber(string phoneNumber, int start, int length, string word)
    {
        var chars = phoneNumber.ToCharArray();
        
        // Build the vanity number with the word replacing the digits
        var result = "";
        
        for (int i = 0; i < phoneNumber.Length; i++)
        {
            if (i == start)
            {
                result += word;
                i += length - 1; // Skip the digits that were replaced
            }
            else if (i < phoneNumber.Length)
            {
                result += phoneNumber[i];
            }
        }

        return result;
    }

    /// <summary>
    /// Determines which dictionary types contain a specific word.
    /// </summary>
    /// <param name="word">The word to check.</param>
    /// <param name="requestedTypes">The dictionary types to search within.</param>
    /// <returns>A DictionaryType flags enum indicating which dictionaries contain the word.</returns>
    private DictionaryType GetMatchedDictionaries(string word, DictionaryType requestedTypes)
    {
        var matched = DictionaryType.None;

        if (requestedTypes.HasFlag(DictionaryType.Dutch) && _dictionaryService.IsWord(word, DictionaryType.Dutch))
            matched |= DictionaryType.Dutch;
        if (requestedTypes.HasFlag(DictionaryType.English) && _dictionaryService.IsWord(word, DictionaryType.English))
            matched |= DictionaryType.English;
        if (requestedTypes.HasFlag(DictionaryType.Urban) && _dictionaryService.IsWord(word, DictionaryType.Urban))
            matched |= DictionaryType.Urban;

        return matched;
    }
}
