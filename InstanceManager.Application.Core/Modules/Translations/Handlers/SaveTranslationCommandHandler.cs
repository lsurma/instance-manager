using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Modules.Translations.Handlers;

public class SaveTranslationCommandHandler : IRequestHandler<SaveTranslationCommand, Guid>
{
    private readonly InstanceManagerDbContext _context;

    public SaveTranslationCommandHandler(InstanceManagerDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SaveTranslationCommand request, CancellationToken cancellationToken)
    {
        Translation translation;

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            // Update existing
            translation = await _context.Translations
                .FirstOrDefaultAsync(t => t.Id == request.Id.Value, cancellationToken);

            if (translation == null)
            {
                throw new KeyNotFoundException($"Translation with Id {request.Id} not found.");
            }

            translation.InternalGroupName = request.InternalGroupName;
            translation.ResourceName = request.ResourceName;
            translation.TranslationName = request.TranslationName;
            translation.CultureName = request.CultureName;
            translation.Content = request.Content;
            translation.DataSetId = request.DataSetId;
            translation.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            // Create new
            translation = new Translation
            {
                Id = Guid.NewGuid(),
                InternalGroupName = request.InternalGroupName,
                ResourceName = request.ResourceName,
                TranslationName = request.TranslationName,
                CultureName = request.CultureName,
                Content = request.Content,
                DataSetId = request.DataSetId,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "xx"
            };

            _context.Translations.Add(translation);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return translation.Id;
    }
}
