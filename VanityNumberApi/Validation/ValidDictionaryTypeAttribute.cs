using System.ComponentModel.DataAnnotations;
using VanityNumberApi.Core.Models;

namespace VanityNumberApi.Validation;

/// <summary>
/// Validates that a DictionaryType value contains only valid flag combinations.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class ValidDictionaryTypeAttribute : ValidationAttribute
{
    /// <summary>
    /// Validates the dictionary type value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>A ValidationResult indicating success or failure.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DictionaryType dictionaryType)
        {
            // Check if the value is a valid combination of defined flags
            var validFlags = DictionaryType.Dutch | DictionaryType.English | DictionaryType.Urban;
            
            // Allow None or any combination of valid flags
            if (dictionaryType == DictionaryType.None || (dictionaryType & ~validFlags) == 0)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(
                $"Invalid dictionary type combination. Valid values are: None (0), Dutch (1), English (2), Urban (4), or any combination using bitwise OR.");
        }

        return new ValidationResult("Value must be of type DictionaryType.");
    }
}
