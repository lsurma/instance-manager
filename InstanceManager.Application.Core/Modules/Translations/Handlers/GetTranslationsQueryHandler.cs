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
        IQueryable<Translation> query = _context.Translations;

        // Create complete query specification with all configuration in one place
        var options = new QueryOptions<Translation, Guid, TranslationDto>
        {
            AsNoTracking = true,
            Filtering = request.Filtering,
            Ordering = request.Ordering,
            Selector = t => new TranslationDto
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
            }
        };

        // Apply query preparation (authorization, filters, ordering)
        query = await _queryService.PrepareQueryAsync(query, options, cancellationToken);

        // Execute paginated query with projection
        return await _queryService.ExecutePaginatedQueryAsync(
            query,
            request.Pagination,
            options,
            cancellationToken);
    }
}
