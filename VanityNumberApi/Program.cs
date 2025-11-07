using System.Text.Json.Serialization;
using VanityNumberApi.Core.Services;

var builder = WebApplication.CreateBuilder(args);

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
    
    // Include XML comments for Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
});

// Register our services
builder.Services.AddSingleton<IPhoneToLetterMapper, PhoneToLetterMapper>();
builder.Services.AddSingleton<IDictionaryService, DictionaryService>();
builder.Services.AddSingleton<IVanityNumberService, VanityNumberService>();

var app = builder.Build();

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

app.Run();
