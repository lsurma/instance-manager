using InstanceManager.Application.Contracts.ProjectInstance;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.ProjectInstance.Handlers;

public class GetAllProjectInstancesQueryHandler : IRequestHandler<GetAllProjectInstancesQuery, List<ProjectInstanceDto>>
{
    private readonly InstanceManagerDbContext _context;

    public GetAllProjectInstancesQueryHandler(InstanceManagerDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectInstanceDto>> Handle(GetAllProjectInstancesQuery request, CancellationToken cancellationToken)
    {
        var instances = await _context.ProjectInstances
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return instances.ToDto();
    }
}
