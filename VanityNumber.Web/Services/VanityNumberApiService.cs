using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using VanityNumber.Contracts.Models;

namespace VanityNumber.Web.Services;

/// <summary>
/// Service for interacting with the Vanity Number API.
/// </summary>
public class VanityNumberService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="VanityNumberService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public VanityNumberService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        // Configure JSON serialization to match the API (enums as strings)
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Converts a phone number to vanity numbers.
    /// </summary>
    /// <param name="request">The vanity number request.</param>
    /// <returns>The vanity number result.</returns>
    public async Task<VanityNumberResult?> ConvertToVanityNumberAsync(VanityNumberRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/VanityNumber/convert", request, _jsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API returned {response.StatusCode}: {errorContent}");
        }

        return await response.Content.ReadFromJsonAsync<VanityNumberResult>(_jsonOptions);
    }

    /// <summary>
    /// Gets the available dictionaries.
    /// </summary>
    /// <returns>List of available dictionaries.</returns>
    public async Task<List<DictionaryInfo>?> GetDictionariesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<DictionaryInfo>>("api/VanityNumber/dictionaries", _jsonOptions);
    }

    /// <summary>
    /// Converts a vanity number back to digits.
    /// </summary>
    /// <param name="vanityNumber">The vanity number to convert.</param>
    /// <returns>The original digit sequence.</returns>
    public async Task<string?> ConvertVanityToDigitsAsync(string vanityNumber)
    {
        return await _httpClient.GetFromJsonAsync<string>($"api/VanityNumber/toDigits/{vanityNumber}", _jsonOptions);
    }
}
