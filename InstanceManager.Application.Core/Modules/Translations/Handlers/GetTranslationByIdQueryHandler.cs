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
        // Use selector-based GetByIdAsync for database-level projection (more efficient)
        // This applies authorization filtering automatically via TranslationsQueryService
        return await _queryService.GetByIdAsync(
            _context.Translations.AsNoTracking(),
            request.Id,
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
            options: null,
            cancellationToken);
    }
}
