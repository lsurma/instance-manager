using System.Reflection;
using InstanceManager.Application.Contracts;
using InstanceManager.Application.Core.Extensions;
using InstanceManager.Host.AzFuncAPI.Authentication;
using InstanceManager.Host.AzFuncAPI.Middleware;
using InstanceManager.Host.AzFuncAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication()
    .UseMiddleware<ApimBypassMiddleware>();

// Add database
var connectionString = builder.Configuration.GetConnectionString("InstanceManagerDb")
    ?? "Data Source=db/instanceManager.db";
builder.Services.AddInstanceManagerCore(connectionString);

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configure authentication
var authSettings = builder.Configuration.GetSection(AuthenticationSettings.SectionName).Get<AuthenticationSettings>()
    ?? new AuthenticationSettings();

// Add authentication services
var authBuilder = builder.Services.AddAuthentication(options =>
{
    // Set default schemes based on what's enabled
    if (authSettings.EntraId.Enabled)
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
    else if (authSettings.ApiKeys.Enabled)
    {
        options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
        options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
    }
});

// Add Entra ID (Azure AD) JWT Bearer authentication
if (authSettings.EntraId.Enabled && !string.IsNullOrWhiteSpace(authSettings.EntraId.TenantId))
{
    authBuilder.AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("Authentication:EntraId", options);
        options.TokenValidationParameters.ValidAudience = authSettings.EntraId.Audience ?? authSettings.EntraId.ClientId;
    },
    options =>
    {
        builder.Configuration.Bind("Authentication:EntraId", options);
    });
}

// Add API Key authentication
if (authSettings.ApiKeys.Enabled && authSettings.ApiKeys.Keys.Any())
{
    authBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.DefaultScheme,
        options =>
        {
            options.ApiKeys = authSettings.ApiKeys.Keys;
        });
}

// Add authorization
builder.Services.AddAuthorization(options =>
{
    // Default policy - require authentication if enabled (unless APIM bypass is active)
    if (authSettings.RequireAuthentication && !authSettings.Apim.TrustApim)
    {
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    }

    // Policy for API Key authentication
    options.AddPolicy("ApiKeyPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes(ApiKeyAuthenticationOptions.DefaultScheme));

    // Policy for JWT Bearer authentication
    options.AddPolicy("JwtBearerPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));

    // Policy that accepts either authentication method
    options.AddPolicy("ApiOrJwtPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes(ApiKeyAuthenticationOptions.DefaultScheme, JwtBearerDefaults.AuthenticationScheme));
});

// Store settings for later use
builder.Services.AddSingleton(authSettings);

// Find all IRequest from contracts assembly
builder.Services.AddSingleton<RequestRegistry>();


var app = builder.Build();

// Initialize database
await app.Services.InitializeDatabaseAsync();

app.Run();