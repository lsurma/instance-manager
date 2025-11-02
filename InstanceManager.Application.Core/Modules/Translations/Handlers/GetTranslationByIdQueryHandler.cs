using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Modules.Translations.Handlers;

public class GetTranslationByIdQueryHandler : IRequestHandler<GetTranslationByIdQuery, TranslationDto?>
{
    private readonly InstanceManagerDbContext _context;

    public GetTranslationByIdQueryHandler(InstanceManagerDbContext context)
    {
        _context = context;
    }

    public async Task<TranslationDto?> Handle(GetTranslationByIdQuery request, CancellationToken cancellationToken)
    {
        var translation = await _context.Translations
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        return translation?.ToDto();
    }
}
