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

        // Query service automatically applies authorization pre-filtering
        query = await _queryService.PrepareQueryAsync(
            query,
            request.Filtering,
            request.Ordering,
            cancellationToken: cancellationToken);

        return await _queryService.ExecutePaginatedQueryAsync(
            query,
            request.Pagination,
            translations => translations.ToDto(),
            cancellationToken);
    }
}
