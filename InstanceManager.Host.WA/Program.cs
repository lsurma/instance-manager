using InstanceManager.Application.Contracts;
using InstanceManager.Host.WA;
using InstanceManager.Host.WA.DAL;
using InstanceManager.Host.WA.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure authorization with default policy requiring authentication
builder.Services.AddAuthorizationCore(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Configure MSAL authentication
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.LoginMode = "redirect";

    // Add default scopes for API access
    var defaultScopes = builder.Configuration.GetSection("AzureAd:DefaultScopes").Get<string[]>();
    if (defaultScopes != null)
    {
        foreach (var scope in defaultScopes)
        {
            options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
        }
    }
});

// Configure HttpClient to use the API base URL from configuration
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:7233/api/";

// Register InstanceManagerHttpClient as Typed Client
builder.Services.AddHttpClient<InstanceManagerHttpClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Configure BaseAddressAuthorizationMessageHandler to attach access tokens
builder.Services.AddScoped<BaseAddressAuthorizationMessageHandler>();


builder.Services.AddScoped<IRequestSender, HttpRequestSender>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<NavigationHelper>();

builder.Services.AddFluentUIComponents();
builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();
