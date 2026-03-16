using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PokerFrontend;
using PokerFrontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Add HttpClient
var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5100") };
httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
builder.Services.AddScoped(sp => httpClient);

// Add custom services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPokerApiService, PokerApiService>();

await builder.Build().RunAsync();
