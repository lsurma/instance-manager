using InstanceManager.Application.Contracts.ProjectInstance;
using InstanceManager.Application.Core.Data;
using MediatR;

namespace InstanceManager.Application.Core.ProjectInstance.Handlers;

public class DeleteProjectInstanceCommandHandler : IRequestHandler<DeleteProjectInstanceCommand, bool>
{
    private readonly InstanceManagerDbContext _context;

    public DeleteProjectInstanceCommandHandler(InstanceManagerDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteProjectInstanceCommand request, CancellationToken cancellationToken)
    {
        var instance = await _context.ProjectInstances.FindAsync([request.Id], cancellationToken);
        if (instance == null)
        {
            return false;
        }

        _context.ProjectInstances.Remove(instance);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
