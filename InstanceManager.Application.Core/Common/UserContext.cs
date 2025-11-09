using System.Security.Claims;
using InstanceManager.Application.Contracts.Common;

namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Scoped service that holds the current user's ClaimsPrincipal for the request.
/// This is more reliable than IHttpContextAccessor in Azure Functions isolated worker model.
/// </summary>
public class UserContext
{
    /// <summary>
    /// The current user's ClaimsPrincipal, populated by middleware
    /// </summary>
    public ClaimsPrincipal? User { get; set; }

    /// <summary>
    /// Checks if a user principal has been set
    /// </summary>
    public bool HasUser => User != null;

    /// <summary>
    /// Checks if the user is authenticated
    /// </summary>
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
