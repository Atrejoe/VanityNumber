using VanityNumber.Contracts.Models;

namespace VanityNumber.Core.Services;

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
        var result = new VanityNumberResult { OriginalNumber = request.PhoneNumber };

        // Clean the phone number (remove spaces, dashes, etc.)
        var cleanNumber = new string(request.PhoneNumber.Where(char.IsDigit).ToArray());
        
        if (string.IsNullOrEmpty(cleanNumber))
        {
            return result;
        }

        // Validate phone number length (max 10 digits for phone keypad mapping)
        if (cleanNumber.Length < 3)
        {
            throw new ArgumentException("Phone number must contain at least 3 digits after cleaning.", nameof(request));
        }
        
        if (cleanNumber.Length > 10)
        {
            throw new ArgumentException("Phone number must contain at most 10 digits after cleaning.", nameof(request));
        }

        var dictionaryTypes = request.DictionaryTypes;
        var minWordLength = request.MinWordLength ?? 3;
        var maxResults = request.MaxResults ?? 20;
        var useLeetSpeak = request.UseLeetSpeak;

        // Find vanity matches
        var matches = new List<VanityMatch>();

        // Try to find words in different segments of the phone number
        for (int start = 0; start < cleanNumber.Length; start++)
        {
            for (int length = minWordLength; length <= cleanNumber.Length - start && length <= 10; length++)
            {
                var segment = cleanNumber.Substring(start, length);
                var combinations = _letterMapper.GenerateLetterCombinationsWithTracking(segment, useLeetSpeak);

                foreach (var combo in combinations)
                {
                    var foundWords = _dictionaryService.FindWords(new[] { combo.Letters }, dictionaryTypes);
                    foreach (var word in foundWords)
                    {
                        var matchedDictionaries = GetMatchedDictionaries(word, dictionaryTypes);
                        var vanityDisplay = combo.ToVanityDisplay();
                        var vanityNumber = BuildVanityNumberWithTracking(cleanNumber, start, length, vanityDisplay);

                        var entry = _dictionaryService.GetEntry(word, matchedDictionaries);
                        matches.Add(new VanityMatch
                        {
                            VanityNumber = vanityNumber,
                            Word = word,
                            DictionaryType = matchedDictionaries,
                            StartPosition = start,
                            Length = length,
                            Definition = entry?.Definition ?? string.Empty
                        });
                        if (matches.Count >= maxResults * 3) { break; }
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
    /// Builds a vanity number string by replacing a segment of digits with a vanity display.
    /// The vanity display already has leet speak tracking applied (digits where leet was used).
    /// </summary>
    /// <param name="phoneNumber">The original phone number.</param>
    /// <param name="start">The starting position of the segment to replace.</param>
    /// <param name="length">The length of the segment to replace.</param>
    /// <param name="vanityDisplay">The vanity display string (e.g., "8atm4n").</param>
    /// <returns>The vanity number with the vanity display replacing the digit segment.</returns>
    private static string BuildVanityNumberWithTracking(string phoneNumber, int start, int length, string vanityDisplay)
    {
        // Build the vanity number with the vanity display replacing the digits
        var result = new System.Text.StringBuilder();
        
        int index = 0;
        while (index < phoneNumber.Length)
        {
            if (index == start)
            {
                result.Append(vanityDisplay);
                index += length; // Skip the digits that were replaced
            }
            else
            {
                result.Append(phoneNumber[index]);
                index++;
            }
        }

        return result.ToString();
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
