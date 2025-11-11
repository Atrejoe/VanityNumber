namespace VanityNumberApi.DictionarySanitizer;

/// <summary>
/// Provides methods for sanitizing word dictionaries for phone number mapping applications, ensuring that words meet
/// specific criteria and are formatted for further processing.
/// </summary>
/// <remarks>DictionarySanitizer supports multiple input formats and enforces constraints such as word length,
/// alphabetic content, and compatibility with phone keypad mappings. Sanitized output preserves original word casing
/// and diacritics, and is suitable for use in phoneword or mnemonic generation scenarios.</remarks>
public class DictionarySanitizer
{
	private const string ValidPhoneLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private const int MinLength = 2;
	private const int MaxLength = 10;

	/// <summary>
	/// Specifies the format used to represent dictionary entries in text files.
	/// </summary>
	/// <remarks>Use this enumeration to indicate how words and their associated data, such as frequency counts, are
	/// separated in dictionary files. The format determines how each line in the file should be parsed when reading or
	/// writing dictionary data.</remarks>
	public enum DictionaryFormat
	{
		/// <summary>One word per line</summary>
		OneWordPerLine,
		/// <summary>Tab-separated: word[tab]frequency</summary>
		TabSeparated,
		/// <summary>Space-separated: word frequency</summary>
		SpaceSeparated
	}

	/// <summary>
	/// Represents the result of a sanitization operation, including counts and the collection of processed lines.
	/// </summary>
	/// <remarks>Use this class to access details about the input and output of a sanitization process, such as the
	/// number of items before and after sanitization and the mapping of normalized to original values. The format of each
	/// line in the <see cref="Lines"/> property is: NORMALIZED&gt;TAB&lt;ORIGINAL.</remarks>
	public class SanitizationResult
	{
		/// <summary>Number of original lines processed</summary>
		public int OriginalCount { get; set; }
		/// <summary>Number of sanitized lines produced</summary>
		public int SanitizedCount { get; set; }
		/// <summary>Output lines in format: NORMALIZED[tab]ORIGINAL</summary>
		public List<string> Lines { get; set; } = new();
	}

	/// <summary>
	/// Processes a dictionary file and returns a sanitized list of words that meet specified criteria for format, length,
	/// and character validity.
	/// </summary>
	/// <remarks>Words are included in the sanitized output only if they consist solely of alphabetic characters,
	/// fall within the allowed length range, and can be mapped to valid phone digits. The original form of each word is
	/// preserved, preferring lowercase or mixed case over uppercase when duplicates exist. The output is sorted by
	/// normalized word.</remarks>
	/// <param name="inputFilePath">The path to the input dictionary file to be sanitized. The file must be accessible and contain one word per line.</param>
	/// <param name="format">The format specification used to extract words from each line of the dictionary file. Determines how words are
	/// parsed and normalized.</param>
	/// <returns>A SanitizationResult containing the original line count, the count of sanitized words, and a list of sanitized
	/// entries in normalized and original form.</returns>
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
			if (!wordMap.ContainsKey(normalized) || (word.Any(char.IsLower) && wordMap[normalized].All(char.IsUpper)))
			{
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

	/// <summary>
	/// Sanitizes a dictionary file and saves the cleaned output to a specified file in the given format.
	/// </summary>
	/// <remarks>This method reads the input file, applies sanitization based on the specified format, and writes
	/// the result to the output file. The output file will be overwritten if it already exists.</remarks>
	/// <param name="inputFilePath">The path to the input dictionary file to be sanitized. Cannot be null or empty.</param>
	/// <param name="outputFilePath">The path where the sanitized dictionary file will be saved. Cannot be null or empty.</param>
	/// <param name="inputFormat">The format in <paramref name="inputFilePath"/></param>
	public static void SanitizeAndSave(
		string inputFilePath,
		string outputFilePath,
		DictionaryFormat inputFormat)
	{
		var result = SanitizeDictionary(inputFilePath, inputFormat);
		File.WriteAllLines(outputFilePath, result.Lines);
	}

	/// <summary>
	/// Extracts the word from the specified line according to the given dictionary format.
	/// </summary>
	/// <remarks>If the line contains no words according to the specified format, an empty string is returned. The
	/// extraction behavior depends on the value of <paramref name="format"/>: for tab-separated and space-separated
	/// formats, only the first word is extracted; for one-word-per-line, the entire trimmed line is returned.</remarks>
	/// <param name="line">The input line containing one or more words to extract. Cannot be null.</param>
	/// <param name="format">The format that determines how words are separated in the line.</param>
	/// <returns>A string containing the extracted word from the line, trimmed of leading and trailing whitespace.</returns>
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
