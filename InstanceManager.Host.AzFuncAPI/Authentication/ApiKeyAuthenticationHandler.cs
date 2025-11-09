using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InstanceManager.Host.AzFuncAPI.Authentication;

/// <summary>
/// Authentication handler for API Key-based authentication.
/// Validates API keys from the X-API-Key header.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if the API Key header is present
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Validate the API key
        if (!IsValidApiKey(providedApiKey, out var keyName))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        // Create claims for the authenticated user
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, keyName ?? "API"),
            new Claim(ClaimTypes.NameIdentifier, providedApiKey),
            new Claim("ApiKey", providedApiKey),
            new Claim("KeyName", keyName ?? "Unknown")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    /// <summary>
    /// Validates the provided API key against configured keys.
    /// </summary>
    private bool IsValidApiKey(string providedApiKey, out string? keyName)
    {
        keyName = null;

        if (Options.ApiKeys == null || !Options.ApiKeys.Any())
        {
            return false;
        }

        var matchingKey = Options.ApiKeys.FirstOrDefault(k => k.Value == providedApiKey);

        if (matchingKey.Value == null)
        {
            return false;
        }

        keyName = matchingKey.Key;
        return true;
    }
}

/// <summary>
/// Options for API Key authentication
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";

    /// <summary>
    /// Dictionary of API keys (Key: Name, Value: API Key)
    /// </summary>
    public Dictionary<string, string> ApiKeys { get; set; } = new();
}
