using System.Text.Json.Serialization;
using VanityNumber.Core.Services;
using Sentry;

// Initialize Sentry EARLY - before creating builder
// This ensures startup errors and logging are captured
var sentryDsn = Environment.GetEnvironmentVariable("Sentry__Dsn") 
    ?? GetConfigurationValue("Sentry:Dsn");

IDisposable? sentryDisposable = null;

if (!string.IsNullOrWhiteSpace(sentryDsn))
{
    sentryDisposable = SentrySdk.Init(options =>
    {
        options.Dsn = sentryDsn;
        options.Environment = Environment.GetEnvironmentVariable("Sentry__Environment") 
            ?? GetConfigurationValue("Sentry:Environment") 
            ?? "Production";
        options.TracesSampleRate = double.TryParse(
            Environment.GetEnvironmentVariable("Sentry__TracesSampleRate") 
            ?? GetConfigurationValue("Sentry:TracesSampleRate"), 
            out var rate) ? rate : 0.1;
        options.Debug = false; // Can be overridden by env var
        options.SendDefaultPii = false;
        options.AttachStacktrace = true;
        options.MaxBreadcrumbs = 50;
        options.AutoSessionTracking = true;
        options.IsGlobalModeEnabled = true;
        
        // Set release version
        options.Release = typeof(Program).Assembly.GetName().Version?.ToString();
        
        // Add custom tags
        options.SetBeforeSend((sentryEvent, hint) =>
        {
            sentryEvent.SetTag("service", "vanity-number-api");
            return sentryEvent;
        });
    });
}

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy for Blazor WebAssembly app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        // Try to get CORS origins from environment variable first (for Kubernetes)
        var corsOriginsEnv = builder.Configuration["CORS_ORIGINS"];
        string[] corsOrigins;

        if (!string.IsNullOrWhiteSpace(corsOriginsEnv))
        {
            // Split comma-separated environment variable
            corsOrigins = corsOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        else
        {
            // Fall back to appsettings.json configuration
            corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>()
                ?? new[] { "http://localhost:5000" };
        }

        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings for better readability in Swagger UI
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add NSwag/OpenAPI
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Vanity Number API";
    config.Version = "v1";
    config.Description = "API for converting phone numbers to vanity numbers using Dutch, English, and Urban dictionaries.";

    // Use relative URLs - no hardcoded server
    config.PostProcess = document =>
    {
        document.Servers.Clear(); // Remove default server URLs to make it relative
        document.Info.Description += "\n\nAPI uses XML documentation for detailed endpoint descriptions.";
    };

    // XML comments would go here if XML documentation is enabled in the project
});

// Register our services
builder.Services.AddSingleton<IPhoneToLetterMapper, PhoneToLetterMapper>();
builder.Services.AddSingleton<IDictionaryService, DictionaryService>();
builder.Services.AddSingleton<IVanityNumberService, VanityNumberService>();

var app = builder.Build();

// Use Sentry request tracing (only if initialized)
if (sentryDisposable != null)
{
    app.UseSentryTracing();
}

// Configure CORS
app.UseCors("AllowBlazorApp");

// Configure NSwag UI at root
app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.Path = ""; // Serve Swagger UI at the root
    config.DocumentPath = "/swagger/v1/swagger.json";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

// Dispose Sentry on shutdown
sentryDisposable?.Dispose();

// Helper method to get configuration value early (before builder is created)
static string? GetConfigurationValue(string key)
{
    try
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();
        
        return config[key];
    }
    catch
    {
        return null;
    }
}
