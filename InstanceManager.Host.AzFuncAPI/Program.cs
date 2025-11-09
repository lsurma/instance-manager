using System.Reflection;
using InstanceManager.Application.Contracts;
using InstanceManager.Application.Core.Extensions;
using InstanceManager.Host.AzFuncAPI.Authentication;
using InstanceManager.Host.AzFuncAPI.Middleware;
using InstanceManager.Host.AzFuncAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;

var builder = FunctionsApplication.CreateBuilder(args);

// Load authentication settings first (needed for middleware configuration)
var authSettings = builder.Configuration.GetSection(AuthenticationSettings.SectionName).Get<AuthenticationSettings>()
    ?? new AuthenticationSettings();

// Store settings for use in middleware and services
builder.Services.AddSingleton(authSettings);

// Configure Functions Web Application with middleware pipeline
var functionsApp = builder.ConfigureFunctionsWebApplication();

// Add APIM bypass middleware (Functions Worker middleware)
functionsApp.UseWhen<ApimBypassMiddleware>(context =>
{
    var services = context.InstanceServices;
    var settings = services.GetRequiredService<AuthenticationSettings>();
    return settings.Apim.TrustApim;
});

// Add authorization middleware (Functions Worker middleware)
functionsApp.UseMiddleware<FunctionsAuthorizationMiddleware>();

// Add HTTP context accessor for user identity tracking
builder.Services.AddHttpContextAccessor();

// Add database
var connectionString = builder.Configuration.GetConnectionString("InstanceManagerDb")
    ?? "Data Source=db/instanceManager.db";
builder.Services.AddInstanceManagerCore(connectionString, authOptions =>
{
    // Configure root users from configuration or environment
    var rootUsers = builder.Configuration.GetSection("Authorization:RootUsers").Get<string[]>();
    if (rootUsers != null)
    {
        foreach (var userId in rootUsers)
        {
            authOptions.AddRootUser(userId);
        }
    }

    // For development, you can also hard-code root users here:
    // authOptions.AddRootUser("admin@example.com");
    // authOptions.AddRootUser("api-key-admin");
});

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

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
    
    // Default policy - require authentication if enabled (unless APIM bypass is active)
    if (authSettings.RequireAuthentication && !authSettings.Apim.TrustApim)
    {
        options.FallbackPolicy = options.GetPolicy("ApiOrJwtPolicy")!;
        options.DefaultPolicy = options.GetPolicy("ApiOrJwtPolicy")!;
    }
});

// Note: authSettings already registered earlier (line 35)

// Find all IRequest from contracts assembly
builder.Services.AddSingleton<RequestRegistry>();


var app = builder.Build();

// Initialize database
await app.Services.InitializeDatabaseAsync();

app.Run();