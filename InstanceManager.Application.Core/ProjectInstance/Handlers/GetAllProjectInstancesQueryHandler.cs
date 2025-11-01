using System.Linq.Dynamic.Core;
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
        var query = _context.ProjectInstances.AsNoTracking();
        
        // Apply ordering if specified
        if (!string.IsNullOrEmpty(request.OrderBy))
        {
            var orderDirection = string.IsNullOrEmpty(request.OrderDirection) || request.OrderDirection.ToLower() == "asc" 
                ? "ascending" 
                : "descending";
            
            query = query.OrderBy($"{request.OrderBy} {orderDirection}");
        }
        
        var instances = await query.ToListAsync(cancellationToken);

        return instances.ToDto();
    }
}
