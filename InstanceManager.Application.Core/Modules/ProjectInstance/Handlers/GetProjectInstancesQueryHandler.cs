using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.ProjectInstance;
using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;
using InstanceManager.Application.Core.Modules.ProjectInstance.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Modules.ProjectInstance.Handlers;

public class GetProjectInstancesQueryHandler : IRequestHandler<GetProjectInstancesQuery, PaginatedList<ProjectInstanceDto>>
{
    private readonly InstanceManagerDbContext _context;
    private readonly IQueryService<ProjectInstance> _queryService;

    public GetProjectInstancesQueryHandler(InstanceManagerDbContext context, IQueryService<ProjectInstance> queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<PaginatedList<ProjectInstanceDto>> Handle(GetProjectInstancesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ProjectInstances.AsNoTracking();

        query = await _queryService.PrepareQueryAsync(
            query,
            request.Filtering,
            request.Ordering,
            new QueryOptions<ProjectInstance>
            {
                SearchSpecificationFactory = searchTerm => new ProjectInstanceSearchSpecification(searchTerm)
            },
            cancellationToken);

        return await _queryService.ExecutePaginatedQueryAsync(
            query,
            request.Pagination,
            instances => instances.ToDto(),
            cancellationToken);
    }
}
