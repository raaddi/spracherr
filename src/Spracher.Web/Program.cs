using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Spracher.Web;
using Spracher.Web.ApiClient;
using Spracher.Web.Components.Exercises;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var configuredApiAddress = builder.Configuration["ApiBaseAddress"];
var apiBaseAddress = string.IsNullOrWhiteSpace(configuredApiAddress)
    ? new Uri(builder.HostEnvironment.BaseAddress)
    : new Uri(configuredApiAddress, UriKind.Absolute);

builder.Services.AddScoped<BrowserCredentialsHandler>();
builder.Services.AddScoped(serviceProvider => new HttpClient(
    serviceProvider.GetRequiredService<BrowserCredentialsHandler>())
{
    BaseAddress = apiBaseAddress,
});
builder.Services.AddScoped<AntiforgeryTokenProvider>();
builder.Services.AddScoped<JsonApiClient>();
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<LanguagesApiClient>();
builder.Services.AddScoped<VocabularyApiClient>();
builder.Services.AddScoped<ExercisesApiClient>();
builder.Services.AddSingleton<ExerciseRendererCatalog>();
builder.Services.AddScoped<AppSessionState>();
builder.Services.AddScoped<SystemApiClient>();

await builder.Build().RunAsync();
