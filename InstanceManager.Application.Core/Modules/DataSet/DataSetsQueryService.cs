using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;

namespace InstanceManager.Application.Core.Modules.DataSet;

/// <summary>
/// Query service for DataSet entities with authorization filtering
/// </summary>
public class DataSetsQueryService : QueryService<DataSet, Guid>
{
    private readonly InstanceManagerDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public DataSetsQueryService(
        InstanceManagerDbContext context,
        IFilterHandlerRegistry filterHandlerRegistry,
        IAuthorizationService authorizationService)
        : base(filterHandlerRegistry)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    protected override IQueryable<DataSet> DefaultQuery => _context.DataSets;

    /// <summary>
    /// Applies authorization filtering to ensure user only sees datasets they have access to.
    /// This method can be used for both list queries and single entity queries.
    /// </summary>
    public async Task<IQueryable<DataSet>> ApplyAuthorizationAsync(
        IQueryable<DataSet> query,
        CancellationToken cancellationToken = default)
    {
        // Get accessible dataset IDs from authorization service
        // This handles both root access and dataset-level permissions
        var (allAccessible, accessibleIds) = await _authorizationService.GetAccessibleDataSetIdsAsync(cancellationToken);

        // If user has access to all datasets, no filtering needed
        if (allAccessible)
        {
            return query;
        }

        if (accessibleIds.Any())
        {
            // User has access to specific datasets - filter by those
            query = query.Where(ds => accessibleIds.Contains(ds.Id));
        }
        else
        {
            // User has no accessible datasets - return empty query
            // This ensures no data leakage even if authorization returns empty list
            query = query.Where(ds => false);
        }

        return query;
    }

    /// <summary>
    /// Prepares a query with authorization pre-filtering, applying filters, includes, and ordering.
    /// Only datasets the user has access to will be included.
    /// If query is null, uses DefaultQuery from DbContext.
    /// </summary>
    public override async Task<IQueryable<DataSet>> PrepareQueryAsync(
        IQueryable<DataSet>? query = null,
        QueryOptions<DataSet, Guid>? options = null,
        CancellationToken cancellationToken = default)
    {
        query = GetQuery(query);

        // Apply authorization pre-filter first
        query = await ApplyAuthorizationAsync(query, cancellationToken);

        // Call base implementation to apply filters, includes, and ordering
        return await base.PrepareQueryAsync(query, options, cancellationToken);
    }

    public class Options : QueryOptions<DataSet, Guid, DataSet>
    {
        public Options()
        {
            AsNoTracking = true;
        }
    }
}
