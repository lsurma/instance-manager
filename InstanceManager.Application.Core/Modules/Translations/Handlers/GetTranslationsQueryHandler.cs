using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;
using InstanceManager.Application.Core.Modules.Translations.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Modules.Translations.Handlers;

public class GetTranslationsQueryHandler : IRequestHandler<GetTranslationsQuery, PaginatedList<TranslationDto>>
{
    private readonly InstanceManagerDbContext _context;
    private readonly IQueryService _queryService;

    public GetTranslationsQueryHandler(InstanceManagerDbContext context, IQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<PaginatedList<TranslationDto>> Handle(GetTranslationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Translations.AsNoTracking();
        
        // Apply module-specific filters
        if (request.DataSetId.HasValue)
        {
            query = query.Where(t => t.DataSetId == request.DataSetId.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(request.CultureName))
        {
            query = query.Where(t => t.CultureName == request.CultureName);
        }
        
        query = _queryService.PrepareQuery(
            query,
            request.Filtering,
            request.Ordering,
            new QueryOptions<Translation>
            {
                SearchSpecificationFactory = searchTerm => new TranslationSearchSpecification(searchTerm)
            });
        
        return await _queryService.ExecutePaginatedQueryAsync(
            query,
            request.Pagination,
            translations => translations.ToDto(),
            cancellationToken);
    }
}
