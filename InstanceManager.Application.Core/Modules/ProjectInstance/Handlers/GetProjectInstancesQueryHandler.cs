using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.ProjectInstance;
using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;
using MediatR;

namespace InstanceManager.Application.Core.Modules.ProjectInstance.Handlers;

public class GetProjectInstancesQueryHandler : IRequestHandler<GetProjectInstancesQuery, PaginatedList<ProjectInstanceDto>>
{
    private readonly InstanceManagerDbContext _context;
    private readonly IQueryService<ProjectInstance, Guid> _queryService;

    public GetProjectInstancesQueryHandler(InstanceManagerDbContext context, IQueryService<ProjectInstance, Guid> queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<PaginatedList<ProjectInstanceDto>> Handle(GetProjectInstancesQuery request, CancellationToken cancellationToken)
    {
        // Prepare query options
        var options = new QueryOptions<ProjectInstance, Guid>
        {
            AsNoTracking = true,
            Filtering = request.Filtering,
            Ordering = request.Ordering
        };

        // No need to pass query - service uses DefaultQuery with DbContext
        var query = await _queryService.PrepareQueryAsync(options: options, cancellationToken: cancellationToken);

        return await _queryService.ExecutePaginatedQueryAsync<ProjectInstanceDto>(
            query,
            request.Pagination,
            (Func<List<ProjectInstance>, List<ProjectInstanceDto>>)(instances => instances.ToDto()),
            cancellationToken);
    }
}
