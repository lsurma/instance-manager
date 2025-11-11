using System.Linq.Expressions;
using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Common;
using MediatR;

namespace InstanceManager.Application.Core.Modules.Translations.Handlers;

/// <summary>
/// Generic handler for retrieving translations with custom projection
/// </summary>
/// <typeparam name="TProjection">The type to project translation data into</typeparam>
public class GetTranslationsQueryHandler<TProjection> : IRequestHandler<GetTranslationsQuery<TProjection>, PaginatedList<TProjection>>
    where TProjection : ITranslationDto
{
    private readonly TranslationsQueryService _queryService;

    public GetTranslationsQueryHandler(TranslationsQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<PaginatedList<TProjection>> Handle(GetTranslationsQuery<TProjection> request, CancellationToken cancellationToken)
    {
        // Get the selector for this projection type from the static class
        var selectorObject = TranslationProjections.GetSelectorFor(typeof(TProjection));
        var selector = (Expression<Func<Translation, TProjection>>)selectorObject!;

        // Create complete query specification with projection
        var options = new QueryOptions<Translation, Guid, TProjection>
        {
            AsNoTracking = true,
            Filtering = request.Filtering,
            Ordering = request.Ordering,
            Selector = selector
        };

        // Apply query preparation (authorization, filters, ordering)
        var query = await _queryService.PrepareQueryAsync(options: options, cancellationToken: cancellationToken);

        // Execute paginated query with projection
        return await _queryService.ExecutePaginatedQueryAsync(
            query,
            request.Pagination,
            options,
            cancellationToken);
    }
}
