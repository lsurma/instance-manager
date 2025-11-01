using System.Linq.Dynamic.Core;
using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.DataSet;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.DataSet.Handlers;

public class GetDataSetsQueryHandler : IRequestHandler<GetDataSetsQuery, PaginatedList<DataSetDto>>
{
    private readonly InstanceManagerDbContext _context;

    public GetDataSetsQueryHandler(InstanceManagerDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<DataSetDto>> Handle(GetDataSetsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DataSets.AsNoTracking();
        
        // Apply full-text search if specified
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(d => 
                d.Name.ToLower().Contains(searchTerm) ||
                (d.Description != null && d.Description.ToLower().Contains(searchTerm)) ||
                (d.Notes != null && d.Notes.ToLower().Contains(searchTerm))
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
        var dataSets = await query
            .Include(ds => ds.Includes)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = dataSets.ToDto();
        
        return new PaginatedList<DataSetDto>(dtos, totalItems, request.PageNumber, request.PageSize);
    }
}
