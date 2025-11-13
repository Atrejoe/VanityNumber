using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VanityNumber.Web;
using VanityNumber.Web.Services;

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

await builder.Build().RunAsync();
