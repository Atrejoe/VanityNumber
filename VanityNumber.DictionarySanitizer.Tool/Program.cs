using CommandLine;

namespace VanityNumber.DictionarySanitizer.Tool;

/// <summary>
/// Main program class for the Dictionary Sanitizer Tool. Provides download, sanitize, and optional enrichment
/// (definitions) for Dutch, English, and Urban dictionary sources.
/// </summary>
public static class Program
{
	/// <summary>
	/// Application entry point. Parses command line options and executes workflow.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	/// <returns>Exit code (0 = success, non-zero = failure).</returns>
	public static async Task<int> Main(string[] args)
	{
		Console.WriteLine("=== Dictionary Sanitizer ===\n");
		return await Parser.Default
			.ParseArguments<Options>(args)
			.MapResult(
				(Options opt) => RunAsync(opt),
				errs => Task.FromResult(1));
	}

	private static async Task<int> RunAsync(Options opt)
	{
		var dictionariesPath = string.IsNullOrWhiteSpace(opt.Path)
			? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "VanityNumber.Core", "Dictionaries")
			: opt.Path;
		dictionariesPath = Path.GetFullPath(dictionariesPath);

		Console.WriteLine($"Dictionaries path: {dictionariesPath}\n");
		Console.WriteLine($"Options: parallel={opt.Parallel}, maxWords={(opt.MaxWords?.ToString() ?? "ALL")}, delayMs={opt.DelayMs}, skipEnrich={opt.SkipEnrich}, urbanFile={(opt.UrbanFile ?? "(auto)")}, urbanUrl={opt.UrbanUrl}, maxDefinitionLen={opt.MaxDefinitionLength}\n");

		if (!Directory.Exists(dictionariesPath))
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Error: Dictionaries path not found: {dictionariesPath}");
			Console.ResetColor();
			return 1;
		}

		await DownloadDictionariesAsync(dictionariesPath, opt);
		await SanitizeAsync(Path.Combine(dictionariesPath, "dutch_raw.txt"), Path.Combine(dictionariesPath, "dutch.txt"), VanityNumber.DictionarySanitizer.DictionarySanitizer.DictionaryFormat.SpaceSeparated, "Dutch");
		await SanitizeAsync(Path.Combine(dictionariesPath, "english_raw.txt"), Path.Combine(dictionariesPath, "english.txt"), VanityNumber.DictionarySanitizer.DictionarySanitizer.DictionaryFormat.SpaceSeparated, "English");
		// Urban: one word per line
		if (File.Exists(Path.Combine(dictionariesPath, "urban_raw.txt")))
		{
			await SanitizeAsync(Path.Combine(dictionariesPath, "urban_raw.txt"), Path.Combine(dictionariesPath, "urban.txt"), VanityNumber.DictionarySanitizer.DictionarySanitizer.DictionaryFormat.OneWordPerLine, "Urban");
		}

		if (!opt.SkipEnrich)
		{
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Enriching dictionaries with definitions...");
			Console.ResetColor();
			await EnrichAsyncSafe(Path.Combine(dictionariesPath, "dutch.txt"), Path.Combine(dictionariesPath, "dutch.txt"), "nl", opt);
			await EnrichAsyncSafe(Path.Combine(dictionariesPath, "english.txt"), Path.Combine(dictionariesPath, "english.txt"), "en", opt);
			var urbanPath = opt.UrbanFile ?? Path.Combine(dictionariesPath, "urban.txt");
			if (File.Exists(urbanPath))
				await EnrichAsyncSafe(urbanPath, urbanPath, "urban", opt);
		}
		else
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine("Skipping enrichment (--skip-enrich)");
			Console.ResetColor();
		}

		Console.WriteLine();
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("=== Sanitization Complete ===");
		if (!opt.SkipEnrich)
			Console.WriteLine("=== Sanitization + Enrichment Complete ===");
		Console.ResetColor();
		return 0;
	}

	private static async Task DownloadDictionariesAsync(string dictionariesPath, Options opt)
	{
		using var httpClient = new HttpClient();
		var dutchUrl = "https://raw.githubusercontent.com/hermitdave/FrequencyWords/master/content/2018/nl/nl_50k.txt";
		var englishUrl = "https://raw.githubusercontent.com/hermitdave/FrequencyWords/master/content/2018/en/en_50k.txt";
		await DownloadIfMissing(httpClient, dutchUrl, Path.Combine(dictionariesPath, "dutch_raw.txt"), "Dutch");
		await DownloadIfMissing(httpClient, englishUrl, Path.Combine(dictionariesPath, "english_raw.txt"), "English");

		// Urban dictionary optional download (public word list; words only)
		if (!string.IsNullOrWhiteSpace(opt.UrbanUrl))
		{
			var urbanRaw = Path.Combine(dictionariesPath, "urban_raw.txt");
			await DownloadIfMissing(httpClient, opt.UrbanUrl, urbanRaw, "Urban");
		}
	}

	private static async Task DownloadIfMissing(HttpClient client, string url, string path, string label)
	{
		if (File.Exists(path))
			return;
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine($"Downloading {label} source...");
		Console.ResetColor();
		try
		{
			var content = await client.GetStringAsync(url);
			await File.WriteAllTextAsync(path, content);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"  ✓ {label} raw file saved: {Path.GetFileName(path)}");
			Console.ResetColor();
		}
		catch (Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"  ✗ Failed to download {label}: {ex.Message}");
			Console.ResetColor();
		}
	}

	private static async Task SanitizeAsync(string input, string output, VanityNumber.DictionarySanitizer.DictionarySanitizer.DictionaryFormat format, string label)
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine($"Sanitizing {label} dictionary...");
		Console.ResetColor();
		if (!File.Exists(input))
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"  ✗ Missing raw file: {input}");
			Console.ResetColor();
			return;
		}
		VanityNumber.DictionarySanitizer.DictionarySanitizer.SanitizeAndSave(input, output, format);
		var count = (await File.ReadAllLinesAsync(output)).Length;
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine($"  ✓ Saved {count:N0} {label} entries to {Path.GetFileName(output)}");
		Console.ResetColor();
	}

	private static async Task EnrichAsyncSafe(string input, string output, string lang, Options opt, CancellationToken cancellationToken = default)
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine($"Adding definitions: {Path.GetFileName(input)} ({lang})...");
		Console.ResetColor();
		if (!File.Exists(input))
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine($"  ! Skipped (file not found): {input}");
			Console.ResetColor();
			return;
		}
		var start = DateTime.UtcNow;
		int lastPrinted = -1;

		var allLines = await File.ReadAllLinesAsync(input, cancellationToken);
		if (opt.Parallel > 1)
		{
			await DefinitionEnhancer.EnrichAsyncParallel(allLines, output, lang, opt.Parallel, opt.MaxWords, opt.DelayMs, progress: (done, total) => PrintProgress(done, total, ref lastPrinted));
		}
		else
		{
			await DefinitionEnhancer.EnrichAsync(allLines, output, lang, opt.MaxWords, opt.DelayMs, progress: (done, total) => PrintProgress(done, total, ref lastPrinted));
		}
		var elapsed = DateTime.UtcNow - start;
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine($"  ✓ Definitions added ({elapsed.TotalMinutes:F1} min)");
		Console.ResetColor();
	}

	private static void PrintProgress(int done, int total, ref int lastPrinted)
	{
		var percent = total == 0 ? 100 : (int)(done * 100.0 / total);
		if (percent != lastPrinted)
		{
			lastPrinted = percent;
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine($"    Progress: {percent,3}% ({done:N0}/{total:N0})");
			Console.ResetColor();
		}
	}

	/// <summary>Command line options for the dictionary sanitizer tool.</summary>
	public class Options
	{
		/// <summary>Degree of parallelism used during enrichment requests.</summary>
		[Option('p', "parallel", HelpText = "Degree of parallelism for enrichment (default 20)", Default = 100)]
		public int Parallel { get; set; }
		/// <summary>Optional maximum number of words to enrich. If omitted all words are processed.</summary>
		[Option('m', "max", HelpText = "Limit number of words to enrich (ALL if omitted)")]
		public int? MaxWords { get; set; }
		/// <summary>Delay in milliseconds between each definition request (throttling).</summary>
		[Option('d', "delay", HelpText = "Delay per request in ms (default 5)", Default = 5)]
		public int DelayMs { get; set; }
		/// <summary>Skip enrichment phase (definitions will not be added).</summary>
		[Option('s', "skip-enrich", HelpText = "Skip definition enrichment", Default = false)]
		public bool SkipEnrich { get; set; }
		/// <summary>Output path for dictionaries (defaults to path relative to tool).</summary>
		[Option('l', "path", HelpText = "Dictionaries output path (default: relative to tool)")]
		public string? Path { get; set; }
		/// <summary>Explicit urban dictionary file path used for enrichment after sanitization.</summary>
		[Option('u', "urban", HelpText = "Path to urban dictionary file (defaults to urban.txt if present)")]
		public string? UrbanFile { get; set; }
		/// <summary>URL to download urban word list (plain one-word-per-line format).</summary>
		[Option("urban-url", HelpText = "URL to download urban word list (one word per line)", Default = "https://raw.githubusercontent.com/tckmn/urban-dictionary-words/master/words")]
		public string UrbanUrl { get; set; } = string.Empty;
		/// <summary>Reserved: Maximum definition length (currently informational).</summary>
		[Option("max-def-len", HelpText = "(Reserved) Max definition length", Default = 240)]
		public int MaxDefinitionLength { get; set; }
	}
}
