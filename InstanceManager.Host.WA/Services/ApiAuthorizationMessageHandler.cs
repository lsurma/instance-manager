using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace InstanceManager.Host.WA.Services;

/// <summary>
/// A custom <see cref="AuthorizationMessageHandler"/> that attaches access tokens to outgoing HTTP requests
/// for external API endpoints (not limited to the application's base URI).
/// </summary>
public class ApiAuthorizationMessageHandler : AuthorizationMessageHandler
{
    /// <summary>
    /// Initializes a new instance of <see cref="ApiAuthorizationMessageHandler"/>.
    /// </summary>
    /// <param name="provider">The <see cref="IAccessTokenProvider"/> to use for requesting tokens.</param>
    /// <param name="navigationManager">The <see cref="NavigationManager"/>.</param>
    /// <param name="apiBaseUrl">The base URL of the API that should receive authorization tokens.</param>
    /// <param name="scopes">Optional scopes to request for the access token.</param>
    public ApiAuthorizationMessageHandler(
        IAccessTokenProvider provider, 
        NavigationManager navigationManager,
        string apiBaseUrl,
        string[]? scopes = null)
        : base(provider, navigationManager)
    {
        ConfigureHandler(
            authorizedUrls: new[] { apiBaseUrl, "https://graph.microsoft.com" },
            scopes: scopes ?? Array.Empty<string>()
        );
    }
}

