using InstanceManager.Application.Contracts.ProjectInstance;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.ProjectInstance.Handlers;

public class GetProjectInstanceByIdQueryHandler : IRequestHandler<GetProjectInstanceByIdQuery, ProjectInstanceDto?>
{
    private readonly InstanceManagerDbContext _context;

    public GetProjectInstanceByIdQueryHandler(InstanceManagerDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectInstanceDto?> Handle(GetProjectInstanceByIdQuery request, CancellationToken cancellationToken)
    {
        var instance = await _context.ProjectInstances
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        return instance?.ToDto();
    }
}
