namespace InstanceManager.Application.Contracts.Common;

public record PaginationParameters
{
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
    /// Creates pagination parameters for fetching all items
    /// </summary>
    public static PaginationParameters AllItems() => new()
    {
        PageNumber = 1,
        PageSize = int.MaxValue
    };
}
