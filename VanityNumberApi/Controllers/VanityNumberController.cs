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
public class VanityNumberController : ControllerBase
{
    private readonly IVanityNumberService _vanityNumberService;
    private readonly ILogger<VanityNumberController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VanityNumberController"/> class.
    /// </summary>
    /// <param name="vanityNumberService">The vanity number generation service.</param>
    /// <param name="logger">The logger instance.</param>
    public VanityNumberController(
        IVanityNumberService vanityNumberService,
        ILogger<VanityNumberController> logger)
    {
        _vanityNumberService = vanityNumberService;
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
    /// <returns>Vanity number matches</returns>
    /// <response code="200">Successfully generated vanity numbers</response>
    /// <response code="400">Invalid parameters</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/VanityNumber/convert/0612345678?dictionaries=Dutch|English&amp;maxResults=5
    ///     
    /// Dictionary types can be specified as:
    /// - Individual: Dutch, English, or Urban
    /// - Combined with pipe: Dutch|English
    /// - Combined with comma: Dutch,English,Urban
    /// - All dictionaries (default if not specified)
    /// </remarks>
    [HttpGet("convert/{phoneNumber}")]
    [ProducesResponseType(typeof(VanityNumberResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<VanityNumberResult> ConvertToVanityNumberGet(
        string phoneNumber,
        [FromQuery] string? dictionaries = null,
        [FromQuery] int minWordLength = 3,
        [FromQuery] int maxResults = 20)
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
}
