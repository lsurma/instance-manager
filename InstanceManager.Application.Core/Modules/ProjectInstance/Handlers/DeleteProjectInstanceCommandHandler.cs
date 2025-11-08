using InstanceManager.Application.Contracts.Modules.ProjectInstance;
using InstanceManager.Application.Core.Data;
using MediatR;

namespace InstanceManager.Application.Core.Modules.ProjectInstance.Handlers;

public class DeleteProjectInstanceCommandHandler : IRequestHandler<DeleteProjectInstanceCommand, bool>
{
    private readonly InstanceManagerDbContext _context;
    private readonly ProjectInstancesQueryService _queryService;

    public DeleteProjectInstanceCommandHandler(InstanceManagerDbContext context, ProjectInstancesQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<bool> Handle(DeleteProjectInstanceCommand request, CancellationToken cancellationToken)
    {
        var instance = await _queryService.GetByIdAsync(
            request.Id,
            cancellationToken: cancellationToken
        );

        if (instance == null)
        {
            return false;
        }

        _context.ProjectInstances.Remove(instance);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
