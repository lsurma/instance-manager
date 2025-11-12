using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;
using MediatR;

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
        // Use QueryOptions with Selector for database-level projection (Specification Pattern)
        // This applies authorization filtering automatically via TranslationsQueryService
        var options = new QueryOptions<Translation, Guid, TranslationDto>
        {
            AsNoTracking = true,
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

        // No need to pass _context.Translations - the service uses its DefaultQuery
        return await _queryService.GetByIdAsync(
            request.Id,
            options: options,
            cancellationToken: cancellationToken);
    }
}
