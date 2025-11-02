using System.Linq.Dynamic.Core;
using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Modules.Translations.Handlers;

public class GetTranslationsQueryHandler : IRequestHandler<GetTranslationsQuery, PaginatedList<TranslationDto>>
{
    private readonly InstanceManagerDbContext _context;

    public GetTranslationsQueryHandler(InstanceManagerDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TranslationDto>> Handle(GetTranslationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Translations.AsNoTracking();
        
        // Filter by DataSetId if specified
        if (request.DataSetId.HasValue)
        {
            query = query.Where(t => t.DataSetId == request.DataSetId.Value);
        }
        
        // Filter by CultureName if specified
        if (!string.IsNullOrWhiteSpace(request.CultureName))
        {
            query = query.Where(t => t.CultureName == request.CultureName);
        }
        
        // Apply full-text search if specified
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t => 
                t.InternalGroupName.ToLower().Contains(searchTerm) ||
                t.ResourceName.ToLower().Contains(searchTerm) ||
                t.TranslationName.ToLower().Contains(searchTerm) ||
                t.Content.ToLower().Contains(searchTerm)
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
        var translations = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = translations.ToDto();
        
        return new PaginatedList<TranslationDto>(dtos, totalItems, request.PageNumber, request.PageSize);
    }
}
