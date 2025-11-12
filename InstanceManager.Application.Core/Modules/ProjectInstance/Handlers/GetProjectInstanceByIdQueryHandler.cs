using InstanceManager.Application.Contracts.Modules.ProjectInstance;
using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;
using MediatR;

namespace InstanceManager.Application.Core.Modules.ProjectInstance.Handlers;

public class GetProjectInstanceByIdQueryHandler : IRequestHandler<GetProjectInstanceByIdQuery, ProjectInstanceDto?>
{
    private readonly InstanceManagerDbContext _context;
    private readonly ProjectInstancesQueryService _queryService;

    public GetProjectInstanceByIdQueryHandler(InstanceManagerDbContext context, ProjectInstancesQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<ProjectInstanceDto?> Handle(GetProjectInstanceByIdQuery request, CancellationToken cancellationToken)
    {
        var options = new QueryOptions<ProjectInstance, Guid>
        {
            AsNoTracking = true
        };

        var instance = await _queryService.GetByIdAsync(
            request.Id,
            options: options,
            cancellationToken: cancellationToken
        );

        return instance?.ToDto();
    }
}
