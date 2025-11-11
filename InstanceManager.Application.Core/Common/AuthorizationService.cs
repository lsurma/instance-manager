using InstanceManager.Authentication.Core;
using InstanceManager.Application.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Production implementation of authorization service with root access support
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly InstanceManagerDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly AuthorizationOptions _options;

    public AuthorizationService(
        InstanceManagerDbContext context,
        ICurrentUserService currentUserService,
        AuthorizationOptions options)
    {
        _context = context;
        _currentUserService = currentUserService;
        _options = options;
    }

    /// <summary>
    /// Checks if the current user has root/admin access to all resources
    /// </summary>
    public Task<bool> HasRootAccessAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        // System users always have root access
        if (currentUser.AuthenticationMethod == AuthenticationMethod.System)
        {
            return Task.FromResult(true);
        }

        // Check if user ID is in the root access list
        var hasRootAccess = _options.RootUserIds.Contains(currentUser.UserId);
        return Task.FromResult(hasRootAccess);
    }

    /// <summary>
    /// Checks if the current user has access to a specific DataSet
    /// </summary>
    public async Task<bool> CanAccessDataSetAsync(Guid dataSetId, CancellationToken cancellationToken = default)
    {
        // Root users have access to everything
        if (await HasRootAccessAsync(cancellationToken))
        {
            return true;
        }

        var currentUser = _currentUserService.GetCurrentUser();

        // Query the DataSet to check if user is in AllowedIdentityIds
        // Pull the collection to do in-memory check (EF Core can't translate Contains on converted collection)
        var dataSet = await _context.DataSets
            .Where(ds => ds.Id == dataSetId)
            .Select(ds => new { ds.AllowedIdentityIds })
            .FirstOrDefaultAsync(cancellationToken);

        if (dataSet == null)
        {
            return false; // DataSet not found
        }

        // If AllowedIdentityIds is empty, treat as public access (no restrictions)
        if (!dataSet.AllowedIdentityIds.Any())
        {
            return true;
        }

        // Check if current user's ID is in the allowed list (in-memory)
        return dataSet.AllowedIdentityIds.Contains(currentUser.UserId);
    }

    /// <summary>
    /// Gets the list of DataSet IDs that the current user has access to.
    /// Returns a tuple where:
    /// - AllAccessible: true if user has access to ALL datasets (no filtering needed), false otherwise
    /// - AccessibleIds: list of accessible dataset IDs (empty if AllAccessible is true)
    /// </summary>
    public async Task<(bool AllAccessible, List<Guid> AccessibleIds)> GetAccessibleDataSetIdsAsync(CancellationToken cancellationToken = default)
    {
        // Root users have access to all datasets - no need to query
        if (await HasRootAccessAsync(cancellationToken))
        {
            return (AllAccessible: true, AccessibleIds: new List<Guid>());
        }

        var currentUser = _currentUserService.GetCurrentUser();

        // Pull all dataset IDs and AllowedIdentityIds from database
        // We need to do this in-memory because EF Core can't translate Contains() on the converted collection
        var allDataSets = await _context.DataSets
            .Select(ds => new { ds.Id, ds.AllowedIdentityIds })
            .ToListAsync(cancellationToken);

        // Filter in memory:
        // 1. AllowedIdentityIds is empty (public access), OR
        // 2. Current user's ID is in AllowedIdentityIds
        var accessibleDataSetIds = allDataSets
            .Where(ds =>
                ds.AllowedIdentityIds.Contains(currentUser.UserId))
            .Select(ds => ds.Id)
            .ToList();

        return (AllAccessible: false, AccessibleIds: accessibleDataSetIds);
    }
}

/// <summary>
/// Configuration options for authorization service
/// </summary>
public class AuthorizationOptions
{
    /// <summary>
    /// List of user IDs that have root/admin access to all resources
    /// </summary>
    public HashSet<string> RootUserIds { get; set; } = new();

    /// <summary>
    /// Adds a user ID to the root access list
    /// </summary>
    public AuthorizationOptions AddRootUser(string userId)
    {
        RootUserIds.Add(userId);
        return this;
    }
}
