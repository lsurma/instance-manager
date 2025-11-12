using InstanceManager.Authentication.Core.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace InstanceManager.Authentication.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInstanceManagerAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authSettings = configuration.GetSection(AuthenticationSettings.SectionName).Get<AuthenticationSettings>()
            ?? new AuthenticationSettings();

        services.AddSingleton(authSettings);

        var authBuilder = services.AddAuthentication(options =>
        {
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

        if (authSettings.EntraId.Enabled && !string.IsNullOrWhiteSpace(authSettings.EntraId.TenantId))
        {
            authBuilder.AddMicrosoftIdentityWebApi(options =>
            {
                configuration.Bind("Authentication:EntraId", options);
                options.TokenValidationParameters.ValidAudience = authSettings.EntraId.Audience ?? authSettings.EntraId.ClientId;
            },
            options =>
            {
                configuration.Bind("Authentication:EntraId", options);
            });
        }

        if (authSettings.ApiKeys.Enabled && authSettings.ApiKeys.Keys.Any())
        {
            authBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationOptions.DefaultScheme,
                options =>
                {
                    options.ApiKeys = authSettings.ApiKeys.Keys;
                });
        }

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiKeyPolicy", policy =>
                policy.RequireAuthenticatedUser()
                      .AddAuthenticationSchemes(ApiKeyAuthenticationOptions.DefaultScheme));

            options.AddPolicy("JwtBearerPolicy", policy =>
                policy.RequireAuthenticatedUser()
                      .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));

            options.AddPolicy("ApiOrJwtPolicy", policy =>
                policy.RequireAuthenticatedUser()
                      .AddAuthenticationSchemes(ApiKeyAuthenticationOptions.DefaultScheme, JwtBearerDefaults.AuthenticationScheme));

            if (authSettings.RequireAuthentication && !authSettings.Apim.TrustApim)
            {
                options.FallbackPolicy = options.GetPolicy("ApiOrJwtPolicy")!;
                options.DefaultPolicy = options.GetPolicy("ApiOrJwtPolicy")!;
            }
        });

        return services;
    }
}
