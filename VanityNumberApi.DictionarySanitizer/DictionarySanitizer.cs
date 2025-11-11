namespace VanityNumberApi.DictionarySanitizer;

public class DictionarySanitizer
{
    private const string ValidPhoneLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int MinLength = 2;
    private const int MaxLength = 10;

    public enum DictionaryFormat
    {
        /// <summary>One word per line</summary>
        OneWordPerLine,
        /// <summary>Tab-separated: word[tab]frequency</summary>
        TabSeparated,
        /// <summary>Space-separated: word frequency</summary>
        SpaceSeparated
    }

    public class SanitizationResult
    {
        public int OriginalCount { get; set; }
        public int SanitizedCount { get; set; }
        /// <summary>Output lines in format: NORMALIZED[tab]ORIGINAL</summary>
        public List<string> Lines { get; set; } = new();
    }

    public static SanitizationResult SanitizeDictionary(
        string inputFilePath,
        DictionaryFormat format)
    {
        var lines = File.ReadAllLines(inputFilePath);
        // Dictionary: normalized -> original word (preserving diacritics)
        var wordMap = new Dictionary<string, string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var word = ExtractWord(line.Trim(), format);
            
            if (string.IsNullOrWhiteSpace(word))
                continue;

            // Only keep alphabetic words
            if (!word.All(char.IsLetter))
                continue;

            var normalized = RemoveDiacritics(word).ToUpperInvariant();

            // Check length
            if (normalized.Length < MinLength || normalized.Length > MaxLength)
                continue;

            // Verify all characters can be mapped to phone digits
            if (!normalized.All(c => ValidPhoneLetters.Contains(c)))
                continue;

            // Store original form (preserve case and diacritics)
            // Prefer lowercase or mixed case over uppercase
            if (!wordMap.ContainsKey(normalized))
            {
                wordMap[normalized] = word;
            }
            else if (word.Any(char.IsLower) && wordMap[normalized].All(char.IsUpper))
            {
                // Prefer lowercase/mixed case over all uppercase
                wordMap[normalized] = word;
            }
        }

        // Create output: NORMALIZED[tab]ORIGINAL
        var sorted = wordMap
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => $"{kvp.Key}\t{kvp.Value}")
            .ToList();

        return new SanitizationResult
        {
            OriginalCount = lines.Length,
            SanitizedCount = sorted.Count,
            Lines = sorted
        };
    }

    public static void SanitizeAndSave(
        string inputFilePath,
        string outputFilePath,
        DictionaryFormat format)
    {
        var result = SanitizeDictionary(inputFilePath, format);
        File.WriteAllLines(outputFilePath, result.Lines);
    }

    private static string ExtractWord(string line, DictionaryFormat format)
    {
        return format switch
        {
            DictionaryFormat.TabSeparated => line.Split('\t')[0].Trim(),
            DictionaryFormat.SpaceSeparated => line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].Trim(),
            DictionaryFormat.OneWordPerLine => line.Trim(),
            _ => line.Trim()
        };
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }
}
