using Microsoft.AspNetCore.Mvc;
using VanityNumberApi.Core.Models;
using VanityNumberApi.Core.Services;

namespace VanityNumberApi.Controllers;

/// <summary>
/// Controller for converting phone numbers to vanity numbers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
#pragma warning disable S6960 // Controller actions are related to vanity number operations
public class VanityNumberController : ControllerBase
#pragma warning restore S6960
{
    private readonly IVanityNumberService _vanityNumberService;
    private readonly IDictionaryService _dictionaryService;
    private readonly IPhoneToLetterMapper _letterMapper;
    private readonly ILogger<VanityNumberController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VanityNumberController"/> class.
    /// </summary>
    /// <param name="vanityNumberService">The vanity number generation service.</param>
    /// <param name="dictionaryService">The dictionary service.</param>
    /// <param name="letterMapper">The phone to letter mapping service.</param>
    /// <param name="logger">The logger instance.</param>
    public VanityNumberController(
        IVanityNumberService vanityNumberService,
        IDictionaryService dictionaryService,
        IPhoneToLetterMapper letterMapper,
        ILogger<VanityNumberController> logger)
    {
        _vanityNumberService = vanityNumberService;
        _dictionaryService = dictionaryService;
        _letterMapper = letterMapper;
        _logger = logger;
    }

    /// <summary>
    /// Convert a phone number to vanity numbers using dictionary lookups
    /// </summary>
    /// <param name="request">The vanity number conversion request</param>
    /// <returns>Vanity number matches</returns>
    /// <response code="200">Successfully generated vanity numbers</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("convert")]
    [ProducesResponseType(typeof(VanityNumberResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<VanityNumberResult> ConvertToVanityNumber([FromBody] VanityNumberRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return BadRequest("Phone number is required");
        }

        try
        {
            var result = _vanityNumberService.GenerateVanityNumbers(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting phone number to vanity number");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Convert a phone number to vanity numbers using a simple GET request
    /// </summary>
    /// <param name="phoneNumber">The phone number to convert (can include formatting)</param>
    /// <param name="dictionaries">Comma or pipe-separated dictionary types (Dutch, English, Urban). Examples: "Dutch|English", "Dutch,English,Urban", or "All"</param>
    /// <param name="minWordLength">Minimum word length (default: 3, range: 2-15)</param>
    /// <param name="maxResults">Maximum number of results (default: 20, range: 1-100)</param>
    /// <param name="useLeetSpeak">Whether to use leet speak mappings (0=O, 1=I/L, 4=A, 5=S, 7=T, 8=B). Default: false</param>
    /// <returns>Vanity number matches</returns>
    /// <response code="200">Successfully generated vanity numbers</response>
    /// <response code="400">Invalid parameters</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/VanityNumber/convert/0612345678?dictionaries=Dutch|English&amp;maxResults=5
    ///     GET /api/VanityNumber/convert/0612345678?dictionaries=Dutch|English&amp;maxResults=5&amp;useLeetSpeak=true
    ///     
    /// Dictionary types can be specified as:
    /// - Individual: Dutch, English, or Urban
    /// - Combined with pipe: Dutch|English
    /// - Combined with comma: Dutch,English,Urban
    /// - All dictionaries (default if not specified)
    /// 
    /// Leet speak mappings (when useLeetSpeak=true):
    /// - 0 = O, 1 = I or L, 4 = A, 5 = S, 7 = T, 8 = B
    /// </remarks>
    [HttpGet("convert/{phoneNumber}")]
    [ProducesResponseType(typeof(VanityNumberResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<VanityNumberResult> ConvertToVanityNumberGet(
        string phoneNumber,
        [FromQuery] string? dictionaries = null,
        [FromQuery] int minWordLength = 3,
        [FromQuery] int maxResults = 20,
        [FromQuery] bool useLeetSpeak = false)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return BadRequest("Phone number is required");
        }

        var request = new VanityNumberRequest
        {
            PhoneNumber = phoneNumber,
            MinWordLength = minWordLength,
            MaxResults = maxResults,
            UseLeetSpeak = useLeetSpeak,
            DictionaryTypes = DictionaryType.All // Default to all dictionaries
        };

        if (!string.IsNullOrWhiteSpace(dictionaries))
        {
            // Parse the dictionary types from the query string
            var dictionaryTypes = DictionaryType.None;
            var parts = dictionaries.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var part in parts)
            {
                if (Enum.TryParse<DictionaryType>(part.Trim(), true, out var parsed))
                {
                    dictionaryTypes |= parsed;
                }
            }

            if (dictionaryTypes != DictionaryType.None)
            {
                request.DictionaryTypes = dictionaryTypes;
            }
        }

        try
        {
            var result = _vanityNumberService.GenerateVanityNumbers(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting phone number to vanity number");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Convert a vanity number back to its original digits
    /// </summary>
    /// <param name="vanityNumber">The vanity number to convert (e.g., "8atm4n" or "cool")</param>
    /// <returns>The original digit sequence</returns>
    /// <response code="200">Successfully converted vanity number to digits</response>
    /// <response code="400">Invalid vanity number</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/VanityNumber/toDigits/8atm4n
    ///     
    /// Returns: "828646"
    /// 
    /// This endpoint converts any vanity number display format back to its original digits.
    /// It handles:
    /// - Pure letters (e.g., "cool" → "2665")
    /// - Mixed letters and digits from leet speak (e.g., "8atm4n" → "828646")
    /// - Uppercase and lowercase letters
    /// </remarks>
    [HttpGet("toDigits/{vanityNumber}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<string> ConvertVanityToDigits(string vanityNumber)
    {
        if (string.IsNullOrWhiteSpace(vanityNumber))
        {
            return BadRequest("Vanity number is required");
        }

        try
        {
            var digits = _letterMapper.ConvertVanityToDigits(vanityNumber);
            return Ok(digits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting vanity number to digits");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Get a list of available dictionaries
    /// </summary>
    /// <returns>List of available dictionaries with word counts</returns>
    /// <response code="200">Successfully retrieved dictionary list</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/VanityNumber/dictionaries
    ///     
    /// Returns information about each available dictionary including:
    /// - Name: The dictionary name (Dutch, English, Urban)
    /// - Value: The numeric value for the dictionary type
    /// - Description: What the dictionary contains
    /// - WordCount: Approximate number of words in the dictionary
    /// </remarks>
    [HttpGet("dictionaries")]
    [ProducesResponseType(typeof(IEnumerable<DictionaryInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<DictionaryInfo>> GetDictionaries()
    {
        try
        {
            var dictionaries = new[]
            {
                new DictionaryInfo
                {
                    Name = "Dutch",
                    Value = (int)DictionaryType.Dutch,
                    Description = "Dutch language dictionary",
                    WordCount = _dictionaryService.GetWordCount(DictionaryType.Dutch)
                },
                new DictionaryInfo
                {
                    Name = "English",
                    Value = (int)DictionaryType.English,
                    Description = "English language dictionary",
                    WordCount = _dictionaryService.GetWordCount(DictionaryType.English)
                },
                new DictionaryInfo
                {
                    Name = "Urban",
                    Value = (int)DictionaryType.Urban,
                    Description = "Urban slang dictionary",
                    WordCount = _dictionaryService.GetWordCount(DictionaryType.Urban)
                }
            };

            return Ok(dictionaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dictionary list");
            return StatusCode(500, "An error occurred while retrieving the dictionary list");
        }
    }
}
