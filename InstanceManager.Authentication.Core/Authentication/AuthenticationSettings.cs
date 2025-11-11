namespace InstanceManager.Authentication.Core.Authentication;

/// <summary>
/// Configuration settings for authentication
/// </summary>
public class AuthenticationSettings
{
    public const string SectionName = "Authentication";

    /// <summary>
    /// Entra ID (Azure AD) configuration
    /// </summary>
    public EntraIdSettings EntraId { get; set; } = new();

    /// <summary>
    /// API Key configuration
    /// </summary>
    public ApiKeySettings ApiKeys { get; set; } = new();

    /// <summary>
    /// Azure API Management configuration
    /// </summary>
    public ApimSettings Apim { get; set; } = new();

    /// <summary>
    /// Whether to require authentication (default: true)
    /// </summary>
    public bool RequireAuthentication { get; set; } = true;
}

/// <summary>
/// Entra ID (Azure AD) JWT configuration
/// </summary>
public class EntraIdSettings
{
    /// <summary>
    /// Azure AD instance (e.g., https://login.microsoftonline.com/)
    /// </summary>
    public string Instance { get; set; } = "https://login.microsoftonline.com/";

    /// <summary>
    /// Tenant ID (GUID or domain)
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Client ID (Application ID)
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Audience (typically the Client ID or API URI)
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// Whether Entra ID authentication is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;
}

/// <summary>
/// API Key configuration
/// </summary>
public class ApiKeySettings
{
    /// <summary>
    /// Whether API Key authentication is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// List of valid API keys. The key itself is used as the identity.
    /// </summary>
    public List<string> Keys { get; set; } = new();
}

/// <summary>
/// Azure API Management configuration
/// </summary>
public class ApimSettings
{
    /// <summary>
    /// Whether to trust requests from Azure API Management (bypasses authentication)
    /// WARNING: Only enable this if your Function App is secured behind APIM on a private network
    /// </summary>
    public bool TrustApim { get; set; } = false;

    /// <summary>
    /// Secret header value that APIM must send to authenticate itself to the backend
    /// The backend will check for X-APIM-Secret header with this value
    /// </summary>
    public string? SharedSecret { get; set; }

    /// <summary>
    /// Whether to require the shared secret header (recommended: true)
    /// If false, any request will bypass auth when TrustApim is true (use only for testing!)
    /// </summary>
    public bool RequireSharedSecret { get; set; } = true;
}
