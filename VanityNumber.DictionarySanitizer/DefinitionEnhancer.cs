using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Concurrent;

namespace VanityNumber.DictionarySanitizer;

/// <summary>Provides methods to enrich sanitized dictionary files with short definitions from public APIs.</summary>
public static class DefinitionEnhancer
{
	private static readonly HttpClient _http = CreateClient();
	private static readonly object _logLock = new();

	/// <summary>Enriches a dictionary file adding a definition as third tab-separated column (sequential).</summary>
	/// <param name="allLines"></param>
	/// <param name="outputFile">Output file (can overwrite input).</param>
	/// <param name="language">en, nl or urban.</param>
	/// <param name="maxWords">Limit words processed (null = all).</param>
	/// <param name="delayPerRequestMs">Throttle delay per request.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <param name="progress">Optional callback receiving (processed, total) counts for progress reporting.</param>
	public static async Task EnrichAsync(
		string[] allLines,
		string outputFile,
		string language,
		int? maxWords = null,
		int delayPerRequestMs = 150,
		CancellationToken cancellationToken = default,
		Action<int, int>? progress = null)
	{
		var total = maxWords.HasValue ? Math.Min(maxWords.Value, allLines.Count(l => !string.IsNullOrWhiteSpace(l))) : allLines.Count(l => !string.IsNullOrWhiteSpace(l));
		var enriched = new List<string>(allLines.Length);
		int processed = 0;
		int lastPercent = -1;

		foreach (var line in allLines)
		{
			if (cancellationToken.IsCancellationRequested)
				break;
			if (string.IsNullOrWhiteSpace(line))
			{ enriched.Add(line); continue; }
			if (maxWords.HasValue && processed >= maxWords.Value)
			{ enriched.Add(line); continue; }
			var parts = line.Split('\t');
			if (parts.Length < 2)
			{ enriched.Add(line); continue; }
			var normalized = parts[0];
			var original = parts[1];
			string definition = parts.Length > 2 ? parts[2] : string.Empty;
			if (string.IsNullOrWhiteSpace(definition))
			{
				definition = await GetDescription(language, delayPerRequestMs, line, cancellationToken);
			}
			enriched.Add($"{normalized}\t{original}\t{EscapeTabs(definition)}");
			processed++;
			if (progress != null && total > 0)
			{
				var percent = (int)(processed * 100.0 / total);
				if (percent != lastPercent || processed % 500 == 0)
				{
					lastPercent = percent;
					progress(processed, total);
				}
			}
			if (maxWords.HasValue && processed >= maxWords.Value)
				break;
			if (delayPerRequestMs > 0)
				await Task.Delay(delayPerRequestMs, cancellationToken);
		}
		await File.WriteAllLinesAsync(outputFile, enriched, cancellationToken);
		progress?.Invoke(processed, total);
	}

	/// <summary>Enriches file in parallel with bounded concurrency.</summary>
	public static async Task EnrichAsyncParallel(
		string[] allLines,
		string outputFile,
		string language,
		int degreeOfParallelism,
		int? maxWords = null,
		int delayPerRequestMs = 0,
		CancellationToken cancellationToken = default,
		Action<int, int>? progress = null)
	{
		if (degreeOfParallelism <= 1)
		{
			await EnrichAsync(allLines, outputFile, language, maxWords, delayPerRequestMs, cancellationToken, progress);
			return;
		}
		var candidates = new List<(int Index, string Line)>();
		for (int i = 0; i < allLines.Length; i++)
		{
			var l = allLines[i];
			if (!string.IsNullOrWhiteSpace(l))
				candidates.Add((i, l));
		}
		if (maxWords.HasValue && maxWords.Value < candidates.Count)
		{
			candidates = candidates.Take(maxWords.Value).ToList();
		}
		var total = candidates.Count;
		var outputMap = new ConcurrentDictionary<int, string>();
		int processed = 0;
		int lastPercent = -1;
		var semaphore = new SemaphoreSlim(degreeOfParallelism);

		async Task Process((int Index, string Line) item)
		{
			await semaphore.WaitAsync(cancellationToken);
			try
			{
				outputMap[item.Index] = await GetDescription(language, delayPerRequestMs, item.Line, cancellationToken);
			}
			finally
			{
				semaphore.Release();
				var done = Interlocked.Increment(ref processed);
				if (progress != null && total > 0)
				{
					var percent = (int)(done * 100.0 / total);
					if (percent != lastPercent || done % 500 == 0)
					{
						lastPercent = percent;
						progress(done, total);
					}
				}
			}
		}
		var tasks = candidates.Select(Process).ToArray();
		await Task.WhenAll(tasks);
		var final = new string[allLines.Length];
		for (int i = 0; i < allLines.Length; i++)
			final[i] = outputMap.TryGetValue(i, out var enriched) ? enriched : allLines[i];
		await File.WriteAllLinesAsync(outputFile, final, cancellationToken);
		progress?.Invoke(processed, total);
	}

	private static async Task<string> GetDescription(string language, int delayPerRequestMs, string line, CancellationToken cancellationToken)
	{
		var parts = line.Split('\t');
		if (parts.Length < 2)
		{
			return line;
		}
		else
		{
			var normalized = parts[0];
			var original = parts[1];
			string definition = parts.Length > 2 ? parts[2] : string.Empty;
			if (string.IsNullOrWhiteSpace(definition))
			{
				definition = language switch
				{
					"urban" => await GetUrbanDefinitionAsync(original, cancellationToken),
					"en" => await GetWiktionarySummaryAsync("en", original, cancellationToken),
					"nl" => await GetWiktionarySummaryAsync("nl", original, cancellationToken),
					_ => definition
				};
				if (string.IsNullOrWhiteSpace(definition))
					LogWarnEmpty(original, language);
				definition = Truncate(definition, 240);
				if (delayPerRequestMs > 0)
					await Task.Delay(delayPerRequestMs, cancellationToken);
			}
			return $"{normalized}\t{original}\t{EscapeTabs(definition)}";
		}
	}

	private static HttpClient CreateClient()
	{
		var handler = new HttpClientHandler
		{
			AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
		};
		var client = new HttpClient(handler);
		client.DefaultRequestHeaders.UserAgent.ParseAdd("VanityNumberDictionarySanitizer/1.0 (+https://example.com)");
		client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
		client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
		return client;
	}

	private static async Task<string> GetWiktionarySummaryAsync(string lang, string word, CancellationToken ct)
	{
		HttpResponseMessage? response = null;
		try
		{
			var url = $"https://{lang}.wiktionary.org/api/rest_v1/page/definition/{Uri.EscapeDataString(word)}";
			using var req = new HttpRequestMessage(HttpMethod.Get, url);
			req.Headers.Accept.ParseAdd("application/json");
			req.Headers.Add("Api-User-Agent","vanity.wikiscraper@robertsrre.nl");
			response = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

			if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
			{
				var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(5);
				LogRateLimit(url, word, lang, retryAfter);
				await Task.Delay(retryAfter, ct);
				response.Dispose();
				using var retryReq = new HttpRequestMessage(HttpMethod.Get, url);
				retryReq.Headers.Accept.ParseAdd("application/json");
				response = await _http.SendAsync(retryReq, HttpCompletionOption.ResponseHeadersRead, ct); // new response
			}

			if (!response.IsSuccessStatusCode)
			{
				if ((int)response.StatusCode == 403)
				{
					LogForbidden(url, word, lang);
					throw new InvalidOperationException($"403 Forbidden for {url}");
				}
				LogWarnStatus(word, lang, (int)response.StatusCode);
				return string.Empty;
			}

			var stream = await response.Content.ReadAsStreamAsync(ct);
			using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
			if (doc.RootElement.TryGetProperty("extract", out var extract))
			{
				var txt = extract.GetString() ?? string.Empty;
				if (string.IsNullOrWhiteSpace(txt))
					LogWarnEmpty(word, lang);
				return FirstSentence(txt);
			}
			LogWarnEmpty(word, lang);
		}
		catch (InvalidOperationException) { throw; }
		catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
		{
			LogWarnException(word, lang, "Timeout: " + ex.Message);
		}
		catch (Exception ex)
		{
			LogWarnException(word, lang, ex.Message);
		}
		finally
		{
			response?.Dispose();
		}
		return string.Empty;
	}

	private static async Task<string> GetUrbanDefinitionAsync(string word, CancellationToken ct)
	{
		try
		{
			var url = $"https://api.urbandictionary.com/v0/define?term={Uri.EscapeDataString(word)}";
			using var resp = await _http.GetAsync(url, ct);
			if (!resp.IsSuccessStatusCode)
			{
				if ((int)resp.StatusCode == 403)
				{
					LogForbidden(url, word, "urban");
					throw new InvalidOperationException($"403 Forbidden for {url}");
				}
				LogWarnStatus(word, "urban", (int)resp.StatusCode);
				return string.Empty;
			}
			var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
			if (json.TryGetProperty("list", out var list) && list.ValueKind == JsonValueKind.Array && list.GetArrayLength() > 0)
			{
				var first = list[0];
				if (first.TryGetProperty("definition", out var def))
				{
					var text = def.GetString() ?? string.Empty;
					text = text.Replace('\r', ' ').Replace('\n', ' ').Trim();
					if (string.IsNullOrWhiteSpace(text))
						LogWarnEmpty(word, "urban");
					return FirstSentence(text);
				}
			}
			LogWarnEmpty(word, "urban");
		}
		catch (InvalidOperationException) { throw; }
		catch (Exception ex)
		{
			LogWarnException(word, "urban", ex.Message);
		}
		return string.Empty;
	}

	private static void LogRateLimit(string url, string word, string lang, TimeSpan retryAfter)
	{
		lock (_logLock)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine($"  ! RATE LIMIT [{lang}] {word} -> {url} retry in {retryAfter.TotalSeconds:N0}s");
			Console.ResetColor();
		}
	}

	private static void LogForbidden(string url, string word, string lang)
	{
		lock (_logLock)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"  ✗ 403 Forbidden [{lang}] {word} -> {url}");
			Console.ResetColor();
		}
	}

	private static void LogWarnStatus(string word, string lang, int status)
	{
		lock (_logLock)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine($"  ! [{lang}] {word} - HTTP {(status):N0} (no definition)");
			Console.ResetColor();
		}
	}

	private static void LogWarnEmpty(string word, string lang)
	{
		lock (_logLock)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine($"  ! [{lang}] {word} - empty definition");
			Console.ResetColor();
		}
	}

	private static void LogWarnException(string word, string lang, string message)
	{
		lock (_logLock)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine($"  ! [{lang}] {word} - exception: {message}");
			Console.ResetColor();
		}
	}

	private static string FirstSentence(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return string.Empty;
		var terminators = new[] { ". ", "! ", "? " };
		var idx = terminators.Select(t => text.IndexOf(t, StringComparison.Ordinal)).Where(i => i >= 0).DefaultIfEmpty(-1).Min();
		if (idx > 0)
			return text[..(idx + 1)].Trim();
		return text.Length > 280 ? text[..280].Trim() + "…" : text.Trim();
	}

	private static string Truncate(string def, int max)
	{
		if (string.IsNullOrEmpty(def))
			return def;
		if (def.Length <= max)
			return def;
		return def[..max] + "…";
	}

	private static string EscapeTabs(string def) => def?.Replace('\t', ' ') ?? string.Empty;
}
