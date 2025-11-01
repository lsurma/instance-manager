namespace InstanceManager.Application.Contracts.Common;

public class PaginatedList<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public PaginatedList()
    {
    }

    public PaginatedList(List<T> items, int totalItems, int currentPage, int pageSize)
    {
        Items = items;
        TotalItems = totalItems;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    public static PaginatedList<T> Create(IEnumerable<T> source, int currentPage, int pageSize)
    {
        var totalItems = source.Count();
        var items = source
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<T>(items, totalItems, currentPage, pageSize);
    }
}
