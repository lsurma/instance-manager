using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Modules.Translations.Handlers;

public class GetTranslationByIdQueryHandler : IRequestHandler<GetTranslationByIdQuery, TranslationDto?>
{
    private readonly InstanceManagerDbContext _context;
    private readonly TranslationsQueryService _queryService;

    public GetTranslationByIdQueryHandler(InstanceManagerDbContext context, TranslationsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<TranslationDto?> Handle(GetTranslationByIdQuery request, CancellationToken cancellationToken)
    {
        // GetByIdAsync now handles PrepareQueryAsync internally
        // This applies authorization filtering automatically via TranslationsQueryService
        var translation = await _queryService.GetByIdAsync(
            _context.Translations.AsNoTracking(),
            request.Id,
            options: null,
            cancellationToken);

        return translation?.ToDto();
    }
}
