using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Core.Common;

namespace InstanceManager.Application.Core.Modules.Translations;

/// <summary>
/// Specialized query service for Translation entities with authorization pre-filtering
/// </summary>
public class TranslationsQueryService : QueryService<Translation>
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
    /// Prepares a query with authorization pre-filtering.
    /// Only translations from accessible datasets will be included.
    /// </summary>
    public override async Task<IQueryable<Translation>> PrepareQueryAsync(
        IQueryable<Translation> query,
        FilteringParameters filtering,
        OrderingParameters ordering,
        QueryOptions<Translation>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply authorization pre-filter: only include translations from accessible datasets
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

        // Call base implementation to apply filters, ordering, etc.
        return await base.PrepareQueryAsync(query, filtering, ordering, options, cancellationToken);
    }
}
