using System.Linq.Dynamic.Core;
using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.ProjectInstance;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.ProjectInstance.Handlers;

public class GetProjectInstancesQueryHandler : IRequestHandler<GetProjectInstancesQuery, PaginatedList<ProjectInstanceDto>>
{
    private readonly InstanceManagerDbContext _context;

    public GetProjectInstancesQueryHandler(InstanceManagerDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ProjectInstanceDto>> Handle(GetProjectInstancesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ProjectInstances.AsNoTracking();
        
        // Apply full-text search if specified
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchTerm) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                (p.MainHost != null && p.MainHost.ToLower().Contains(searchTerm)) ||
                (p.Notes != null && p.Notes.ToLower().Contains(searchTerm))
            );
        }
        
        // Apply ordering if specified
        if (!string.IsNullOrEmpty(request.OrderBy))
        {
            var orderDirection = string.IsNullOrEmpty(request.OrderDirection) || request.OrderDirection.ToLower() == "asc" 
                ? "ascending" 
                : "descending";
            
            query = query.OrderBy($"{request.OrderBy} {orderDirection}");
        }
        
        // Get total count before pagination
        var totalItems = await query.CountAsync(cancellationToken);
        
        // Apply pagination using Skip property
        var instances = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = instances.ToDto();
        
        return new PaginatedList<ProjectInstanceDto>(dtos, totalItems, request.PageNumber, request.PageSize);
    }
}
