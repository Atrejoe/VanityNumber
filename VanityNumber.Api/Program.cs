using System.Text.Json.Serialization;
using VanityNumber.Core.Services;

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
