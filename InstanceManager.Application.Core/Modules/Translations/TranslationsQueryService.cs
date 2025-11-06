using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Core.Common;

namespace InstanceManager.Application.Core.Modules.Translations;

/// <summary>
/// Specialized query service for Translation entities with authorization pre-filtering
/// </summary>
public class TranslationsQueryService : QueryService<Translation, Guid>
{
    private readonly IAuthorizationService _authorizationService;

    public TranslationsQueryService(
        IFilterHandlerRegistry filterHandlerRegistry,
        IAuthorizationService authorizationService)
        : base(filterHandlerRegistry)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Applies authorization filtering to ensure user only sees translations from accessible datasets.
    /// This method can be used for both list queries and single entity queries.
    /// </summary>
    public async Task<IQueryable<Translation>> ApplyAuthorizationAsync(
        IQueryable<Translation> query,
        CancellationToken cancellationToken = default)
    {
        // Get accessible dataset IDs from authorization service
        var accessibleDataSetIds = await _authorizationService.GetAccessibleDataSetIdsAsync(cancellationToken);

        if (accessibleDataSetIds.Any())
        {
            // User has access to specific datasets - filter by those
            // Note: DataSetId is nullable, so we need to handle that
            query = query.Where(t => t.DataSetId.HasValue && accessibleDataSetIds.Contains(t.DataSetId.Value));
        }
        else
        {
            // User has no accessible datasets - return empty query
            // This ensures no data leakage even if authorization returns empty list
            query = query.Where(t => false);
        }

        return query;
    }

    /// <summary>
    /// Prepares a query with authorization pre-filtering, applying filters, includes, and ordering.
    /// Only translations from accessible datasets will be included.
    /// </summary>
    public override async Task<IQueryable<Translation>> PrepareQueryAsync(
        IQueryable<Translation> query,
        QueryOptions<Translation, Guid>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply authorization pre-filter first
        query = await ApplyAuthorizationAsync(query, cancellationToken);

        // Call base implementation to apply filters, includes, and ordering
        return await base.PrepareQueryAsync(query, options, cancellationToken);
    }
}
