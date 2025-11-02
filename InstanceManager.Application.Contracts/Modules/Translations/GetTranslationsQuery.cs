using InstanceManager.Application.Contracts.Common;
using MediatR;

namespace InstanceManager.Application.Contracts.Modules.Translations;

public class GetTranslationsQuery : IRequest<PaginatedList<TranslationDto>>
{
    public string? SearchTerm { get; set; }
    public string? OrderBy { get; set; }
    public string? OrderDirection { get; set; }
    public Guid? DataSetId { get; set; }
    public string? CultureName { get; set; }
    
    private int _pageNumber = 1;
    private int _skip = 0;
    
    /// <summary>
    /// Page number (1-based). Setting this will calculate Skip automatically.
    /// </summary>
    public int PageNumber 
    { 
        get => _pageNumber;
        set
        {
            _pageNumber = value;
            _skip = (value - 1) * PageSize;
        }
    }
    
    /// <summary>
    /// Number of items to skip (0-based offset). Setting this will calculate PageNumber automatically.
    /// </summary>
    public int Skip 
    { 
        get => _skip;
        set
        {
            _skip = value;
            _pageNumber = PageSize > 0 ? (value / PageSize) + 1 : 1;
        }
    }
    
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// Creates a query that returns all items without pagination
    /// </summary>
    public static GetTranslationsQuery AllItems(string? orderBy = null, string? orderDirection = null)
    {
        return new GetTranslationsQuery
        {
            OrderBy = orderBy,
            OrderDirection = orderDirection,
            PageNumber = 1,
            PageSize = int.MaxValue
        };
    }
}
