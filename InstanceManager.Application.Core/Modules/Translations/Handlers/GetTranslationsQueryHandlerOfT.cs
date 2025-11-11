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
    private readonly ITranslationProjectionMapper<TProjection> _mapper;

    public GetTranslationsQueryHandler(
        TranslationsQueryService queryService,
        ITranslationProjectionMapper<TProjection> mapper)
    {
        _queryService = queryService;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TProjection>> Handle(GetTranslationsQuery<TProjection> request, CancellationToken cancellationToken)
    {
        // Create complete query specification with projection from mapper
        var options = new QueryOptions<Translation, Guid, TProjection>
        {
            AsNoTracking = true,
            Filtering = request.Filtering,
            Ordering = request.Ordering,
            Selector = _mapper.GetSelector()
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
