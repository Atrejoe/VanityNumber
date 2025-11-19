using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VanityNumber.Web;
using VanityNumber.Web.Services;
using Sentry;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for API calls
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001/";
var baseAddress = apiBaseUrl.StartsWith("http") 
    ? new Uri(apiBaseUrl) 
    : new Uri(new Uri(builder.HostEnvironment.BaseAddress), apiBaseUrl);

builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = baseAddress
});

// Register the API service
builder.Services.AddScoped<VanityNumberService>();

// Configure Sentry (optional - only if DSN is provided)
var sentryDsn = builder.Configuration["Sentry:Dsn"];
if (!string.IsNullOrWhiteSpace(sentryDsn))
{
    try
    {
        SentrySdk.Init(options =>
        {
            options.Dsn = sentryDsn;
            options.Environment = builder.Configuration["Sentry:Environment"] ?? builder.HostEnvironment.Environment;
            options.TracesSampleRate = double.TryParse(builder.Configuration["Sentry:TracesSampleRate"], out var rate) ? rate : 1.0;
            options.Debug = bool.TryParse(builder.Configuration["Sentry:Debug"], out var debug) && debug;
            
            // Set release version if available
            options.Release = typeof(Program).Assembly.GetName().Version?.ToString();
            
            // Configure for Blazor WebAssembly
            options.IsGlobalModeEnabled = true;
            options.MaxBreadcrumbs = 50;
            options.AttachStacktrace = true;
            options.AutoSessionTracking = true;
            
            // Add custom tag
            options.DefaultTags.Add("service", "vanity-number-web");
            
            // Filter out common Blazor noise
            options.SetBeforeSend((sentryEvent, hint) =>
            {
                // Don't send events for navigation cancellations
                if (sentryEvent.Exception?.GetType().Name == "NavigationException")
                {
                    return null;
                }
                
                return sentryEvent;
            });
        });
    }
    catch (Exception ex)
    {
        // Log but don't fail startup if Sentry initialization fails
        Console.WriteLine($"Warning: Failed to initialize Sentry: {ex.Message}");
    }
}

await builder.Build().RunAsync();
