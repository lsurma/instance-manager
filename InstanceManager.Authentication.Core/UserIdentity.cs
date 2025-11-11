namespace InstanceManager.Authentication.Core;

/// <summary>
/// Represents the identity of the current user making a request.
/// Works across different authentication methods (JWT, API Key, APIM).
/// </summary>
public record UserIdentity
{
    /// <summary>
    /// Unique identifier for the user (e.g., Entra object ID, API key name, or APIM subscription ID)
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Display name or email of the user (if available)
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Email address of the user (if available)
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Authentication method used (JWT, APIKey, APIM)
    /// </summary>
    public required AuthenticationMethod AuthenticationMethod { get; init; }

    /// <summary>
    /// Additional claims or metadata about the user
    /// </summary>
    public Dictionary<string, string> AdditionalClaims { get; init; } = new();

    /// <summary>
    /// Whether the user is authenticated
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(UserId);

    /// <summary>
    /// Returns a user-friendly string representation for logging
    /// </summary>
    public string ToLogString() =>
        $"{DisplayName ?? UserId} ({AuthenticationMethod})";

    /// <summary>
    /// Creates an anonymous/unauthenticated user identity
    /// </summary>
    public static UserIdentity Anonymous() => new()
    {
        UserId = "anonymous",
        DisplayName = "Anonymous",
        AuthenticationMethod = AuthenticationMethod.None
    };

    /// <summary>
    /// Creates a system identity for automated operations
    /// </summary>
    public static UserIdentity System() => new()
    {
        UserId = "system",
        DisplayName = "System",
        AuthenticationMethod = AuthenticationMethod.System
    };
}

/// <summary>
/// Authentication methods supported by the application
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>
    /// No authentication
    /// </summary>
    None = 0,

    /// <summary>
    /// JWT Bearer token (Entra ID / Azure AD)
    /// </summary>
    JWT = 1,

    /// <summary>
    /// API Key authentication
    /// </summary>
    APIKey = 2,

    /// <summary>
    /// Azure API Management (APIM) gateway
    /// </summary>
    APIM = 3,

    /// <summary>
    /// System/automated operations
    /// </summary>
    System = 99
}
