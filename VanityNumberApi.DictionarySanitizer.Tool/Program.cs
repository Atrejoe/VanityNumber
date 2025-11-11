using VanityNumberApi.DictionarySanitizer;

Console.WriteLine("=== Dictionary Sanitizer ===");
Console.WriteLine();

var dictionariesPath = args.Length > 0 
    ? args[0] 
    : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "VanityNumberApi.Core", "Dictionaries");

dictionariesPath = Path.GetFullPath(dictionariesPath);

if (!Directory.Exists(dictionariesPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: Dictionaries path not found: {dictionariesPath}");
    Console.ResetColor();
    return 1;
}

Console.WriteLine($"Dictionaries path: {dictionariesPath}");
Console.WriteLine();

// Download dictionaries if needed
await DownloadDictionariesAsync(dictionariesPath);

// Sanitize Dutch dictionary (tab-separated format)
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Sanitizing Dutch dictionary...");
Console.ResetColor();

var dutchInput = Path.Combine(dictionariesPath, "dutch_raw.txt");
var dutchOutput = Path.Combine(dictionariesPath, "dutch.txt");

if (File.Exists(dutchInput))
{
    DictionarySanitizer.SanitizeAndSave(
        dutchInput,
        dutchOutput,
        DictionarySanitizer.DictionaryFormat.TabSeparated);
    
    var dutchCount = File.ReadAllLines(dutchOutput).Length;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  ✓ Saved {dutchCount:N0} Dutch words");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  ✗ File not found: {dutchInput}");
    Console.ResetColor();
}

// Sanitize English dictionary (one word per line)
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Sanitizing English dictionary...");
Console.ResetColor();

var englishInput = Path.Combine(dictionariesPath, "english_raw.txt");
var englishOutput = Path.Combine(dictionariesPath, "english.txt");

if (File.Exists(englishInput))
{
    DictionarySanitizer.SanitizeAndSave(
        englishInput,
        englishOutput,
        DictionarySanitizer.DictionaryFormat.OneWordPerLine);
    
    var englishCount = File.ReadAllLines(englishOutput).Length;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  ✓ Saved {englishCount:N0} English words");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  ✗ File not found: {englishInput}");
    Console.ResetColor();
}

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("=== Sanitization Complete ===");
Console.ResetColor();

return 0;

static async Task DownloadDictionariesAsync(string dictionariesPath)
{
    using var httpClient = new HttpClient();
    
    // Dutch dictionary (tab-separated with frequency)
    var dutchUrl = "https://raw.githubusercontent.com/hermitdave/FrequencyWords/master/content/2018/nl/nl_50k.txt";
    var dutchRaw = Path.Combine(dictionariesPath, "dutch_raw.txt");
    
    if (!File.Exists(dutchRaw))
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Downloading Dutch dictionary...");
        Console.ResetColor();
        
        var dutchContent = await httpClient.GetStringAsync(dutchUrl);
        await File.WriteAllTextAsync(dutchRaw, dutchContent);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Downloaded to {dutchRaw}");
        Console.ResetColor();
    }
    
    // English dictionary (one word per line)
    var englishUrl = "https://raw.githubusercontent.com/dwyl/english-words/master/words_alpha.txt";
    var englishRaw = Path.Combine(dictionariesPath, "english_raw.txt");
    
    if (!File.Exists(englishRaw))
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Downloading English dictionary...");
        Console.ResetColor();
        
        var englishContent = await httpClient.GetStringAsync(englishUrl);
        await File.WriteAllTextAsync(englishRaw, englishContent);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Downloaded to {englishRaw}");
        Console.ResetColor();
    }
    
    Console.WriteLine();
}
