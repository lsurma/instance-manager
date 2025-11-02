using InstanceManager.Application.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Mock implementation of authorization service for development/testing
/// Returns a predefined subset of DataSet IDs to simulate user access restrictions
/// </summary>
public class MockAuthorizationService : IAuthorizationService
{
    private readonly InstanceManagerDbContext _context;

    public MockAuthorizationService(InstanceManagerDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns a subset of existing DataSet IDs to simulate authorization.
    /// In a real implementation, this would query user permissions/roles.
    /// </summary>
    public async Task<List<Guid>> GetAccessibleDataSetIdsAsync(CancellationToken cancellationToken = default)
    {
        // For mock purposes, return the first 2 datasets from the database
        // In production, this would check user claims, roles, or permissions
        var accessibleDataSetIds = await _context.DataSets
            .OrderBy(ds => ds.CreatedAt)
            .Take(2)
            .Select(ds => ds.Id)
            .ToListAsync(cancellationToken);

        // If no datasets exist yet, return empty list (no access)
        return accessibleDataSetIds;
    }
}
