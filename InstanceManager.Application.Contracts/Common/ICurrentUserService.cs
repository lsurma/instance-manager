namespace InstanceManager.Application.Contracts.Common;

/// <summary>
/// Service for accessing the current authenticated user's identity.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's identity information.
    /// Returns anonymous user if no authentication is present.
    /// </summary>
    UserIdentity GetCurrentUser();

    /// <summary>
    /// Gets the current user's unique identifier.
    /// </summary>
    string GetUserId();

    /// <summary>
    /// Gets the current user's display name (or UserId if not available).
    /// </summary>
    string GetUserDisplayName();

    /// <summary>
    /// Checks if the current request is authenticated.
    /// </summary>
    bool IsAuthenticated();
}
