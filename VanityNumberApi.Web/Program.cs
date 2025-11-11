using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VanityNumberApi.Web;
using VanityNumberApi.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001/") 
});

// Register the API service
builder.Services.AddScoped<VanityNumberApiService>();

await builder.Build().RunAsync();
