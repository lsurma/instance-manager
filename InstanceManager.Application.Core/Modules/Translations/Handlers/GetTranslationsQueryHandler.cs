using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Modules.Translations.Handlers;

public class GetTranslationsQueryHandler : IRequestHandler<GetTranslationsQuery, PaginatedList<TranslationDto>>
{
    private readonly InstanceManagerDbContext _context;
    private readonly TranslationsQueryService _queryService;

    public GetTranslationsQueryHandler(InstanceManagerDbContext context, TranslationsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<PaginatedList<TranslationDto>> Handle(GetTranslationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Translations.AsNoTracking();

        // Prepare query options
        var options = new QueryOptions<Translation>
        {
            Filtering = request.Filtering,
            Ordering = request.Ordering
        };

        // Query service automatically applies authorization pre-filtering
        query = await _queryService.PrepareQueryAsync(query, options, cancellationToken);

        // Use selector-based method for database-level projection (more efficient)
        return await _queryService.ExecutePaginatedQueryAsync(
            query,
            request.Pagination,
            t => new TranslationDto
            {
                Id = t.Id,
                InternalGroupName = t.InternalGroupName,
                ResourceName = t.ResourceName,
                TranslationName = t.TranslationName,
                CultureName = t.CultureName,
                Content = t.Content,
                DataSetId = t.DataSetId,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CreatedBy = t.CreatedBy
            },
            cancellationToken);
    }
}
