using System.Security.Claims;
using InstanceManager.Authentication.Core;
using Microsoft.AspNetCore.Http;

namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Implementation of ICurrentUserService that extracts user identity from HTTP context or UserContext.
/// Supports JWT, API Key, and APIM authentication methods.
/// In Azure Functions, UserContext is more reliable than IHttpContextAccessor.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserContext _userContext;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, UserContext userContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _userContext = userContext;
    }

    public UserIdentity GetCurrentUser()
    {
        // Try UserContext first (more reliable in Azure Functions)
        if (_userContext.HasUser)
        {
            return ExtractUserIdentityFromPrincipal(_userContext.User!, httpContext: null);
        }

        // Fallback to HttpContext if UserContext is not populated
        var httpContext = _httpContextAccessor.HttpContext;

        // If no HTTP context (e.g., background jobs), return system identity
        if (httpContext == null)
        {
            return UserIdentity.System();
        }

        return ExtractUserIdentityFromPrincipal(httpContext.User, httpContext);
    }

    /// <summary>
    /// Extracts UserIdentity from a ClaimsPrincipal
    /// </summary>
    private UserIdentity ExtractUserIdentityFromPrincipal(ClaimsPrincipal user, HttpContext? httpContext)
    {
        // If not authenticated, return anonymous
        if (user?.Identity?.IsAuthenticated != true)
        {
            return UserIdentity.Anonymous();
        }

        // Determine authentication method and extract identity
        var authMethod = DetermineAuthenticationMethod(user);

        return authMethod switch
        {
            AuthenticationMethod.JWT => ExtractJwtIdentity(user),
            AuthenticationMethod.APIKey => ExtractApiKeyIdentity(user),
            AuthenticationMethod.APIM => ExtractApimIdentity(user, httpContext),
            _ => UserIdentity.Anonymous()
        };
    }

    public string GetUserId()
    {
        return GetCurrentUser().UserId;
    }

    public string GetUserDisplayName()
    {
        var user = GetCurrentUser();
        return user.DisplayName ?? user.UserId;
    }

    public bool IsAuthenticated()
    {
        return GetCurrentUser().IsAuthenticated;
    }

    /// <summary>
    /// Determines which authentication method was used based on claims and scheme
    /// </summary>
    private AuthenticationMethod DetermineAuthenticationMethod(ClaimsPrincipal user)
    {
        // Check for APIM authentication
        if (user.HasClaim(c => c.Type == "AuthenticationMethod" && c.Value == "APIM-Bypass"))
        {
            return AuthenticationMethod.APIM;
        }

        // Check for API Key authentication
        if (user.HasClaim(c => c.Type == "ApiKey") || user.Identity?.AuthenticationType == "ApiKey")
        {
            return AuthenticationMethod.APIKey;
        }

        // Check for JWT authentication (common JWT claims)
        if (user.HasClaim(c => c.Type == "oid") || // Object ID (Azure AD)
            user.HasClaim(c => c.Type == "sub") || // Subject (standard JWT)
            user.Identity?.AuthenticationType == "Bearer" ||
            user.Identity?.AuthenticationType == "AuthenticationTypes.Federation")
        {
            return AuthenticationMethod.JWT;
        }

        return AuthenticationMethod.None;
    }

    /// <summary>
    /// Extracts user identity from JWT token claims
    /// </summary>
    private UserIdentity ExtractJwtIdentity(ClaimsPrincipal user)
    {
        // Azure AD / Entra ID uses 'oid' (object ID) as the unique identifier
        var userId = user.FindFirst("oid")?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? "unknown-jwt-user";

        var displayName = user.FindFirst("name")?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value
            ?? user.FindFirst("preferred_username")?.Value;

        var email = user.FindFirst("email")?.Value
            ?? user.FindFirst(ClaimTypes.Email)?.Value
            ?? user.FindFirst("upn")?.Value; // User Principal Name

        var additionalClaims = new Dictionary<string, string>();

        // Capture common Azure AD claims
        AddClaimIfPresent(user, additionalClaims, "tid", "TenantId");
        AddClaimIfPresent(user, additionalClaims, "app_displayname", "AppDisplayName");
        AddClaimIfPresent(user, additionalClaims, "appid", "AppId");
        AddClaimIfPresent(user, additionalClaims, "roles", "Roles");

        return new UserIdentity
        {
            UserId = userId,
            DisplayName = displayName,
            Email = email,
            AuthenticationMethod = AuthenticationMethod.JWT,
            AdditionalClaims = additionalClaims
        };
    }

    /// <summary>
    /// Extracts user identity from API Key authentication
    /// </summary>
    private UserIdentity ExtractApiKeyIdentity(ClaimsPrincipal user)
    {
        var keyName = user.FindFirst("KeyName")?.Value ?? "unknown-api-key";
        var apiKey = user.FindFirst("ApiKey")?.Value;

        var additionalClaims = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            // Store a hashed version or just the last 4 characters for logging
            additionalClaims["ApiKeyHash"] = $"***{apiKey[^Math.Min(4, apiKey.Length)..]}";
        }

        return new UserIdentity
        {
            UserId = keyName,
            DisplayName = keyName,
            AuthenticationMethod = AuthenticationMethod.APIKey,
            AdditionalClaims = additionalClaims
        };
    }

    /// <summary>
    /// Extracts user identity from APIM gateway headers
    /// </summary>
    private UserIdentity ExtractApimIdentity(ClaimsPrincipal user, HttpContext? httpContext)
    {
        // APIM can forward user identity via headers
        var userId = httpContext?.Request.Headers["X-User-Id"].FirstOrDefault()
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "apim-gateway";

        var displayName = httpContext?.Request.Headers["X-User-Name"].FirstOrDefault()
            ?? httpContext?.Request.Headers["X-User-Email"].FirstOrDefault()
            ?? user.FindFirst(ClaimTypes.Name)?.Value;

        var email = httpContext?.Request.Headers["X-User-Email"].FirstOrDefault()
            ?? user.FindFirst(ClaimTypes.Email)?.Value;

        var additionalClaims = new Dictionary<string, string>();

        // Capture APIM-specific headers if httpContext is available
        if (httpContext != null)
        {
            var subscriptionName = httpContext.Request.Headers["X-Subscription-Name"].FirstOrDefault();
            var subscriptionId = httpContext.Request.Headers["X-Subscription-Id"].FirstOrDefault();
            var authMethod = httpContext.Request.Headers["X-Auth-Method"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(subscriptionName))
                additionalClaims["SubscriptionName"] = subscriptionName;

            if (!string.IsNullOrWhiteSpace(subscriptionId))
                additionalClaims["SubscriptionId"] = subscriptionId;

            if (!string.IsNullOrWhiteSpace(authMethod))
                additionalClaims["OriginalAuthMethod"] = authMethod;
        }

        return new UserIdentity
        {
            UserId = userId,
            DisplayName = displayName,
            Email = email,
            AuthenticationMethod = AuthenticationMethod.APIM,
            AdditionalClaims = additionalClaims
        };
    }

    /// <summary>
    /// Helper method to add claim to dictionary if it exists
    /// </summary>
    private void AddClaimIfPresent(ClaimsPrincipal user, Dictionary<string, string> claims, string claimType, string key)
    {
        var value = user.FindFirst(claimType)?.Value;
        if (!string.IsNullOrWhiteSpace(value))
        {
            claims[key] = value;
        }
    }
}
