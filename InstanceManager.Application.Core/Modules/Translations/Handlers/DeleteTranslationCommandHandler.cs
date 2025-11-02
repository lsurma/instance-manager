using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Data;
using MediatR;

namespace InstanceManager.Application.Core.Modules.Translations.Handlers;

public class DeleteTranslationCommandHandler : IRequestHandler<DeleteTranslationCommand, bool>
{
    private readonly InstanceManagerDbContext _context;

    public DeleteTranslationCommandHandler(InstanceManagerDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteTranslationCommand request, CancellationToken cancellationToken)
    {
        var translation = await _context.Translations.FindAsync([request.Id], cancellationToken);

        if (translation == null)
        {
            return false;
        }

        _context.Translations.Remove(translation);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
