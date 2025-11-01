# Pagination Quick Start

## Simple Usage

### Full-Text Search
```csharp
var query = new GetProjectInstancesQuery
{
    SearchTerm = "production",  // Searches Name, Description, MainHost, Notes
    PageSize = 20
};

var result = await mediator.Send(query);
// Returns only items matching the search term
```

### Get Paginated Results (Data Grid)
```csharp
// Using PageNumber
var query = new GetProjectInstancesQuery
{
    PageNumber = 1,
    PageSize = 20
};

// OR using Skip (offset-based, easier for DataGrid)
var query = new GetProjectInstancesQuery
{
    Skip = 0,  // Number of items to skip
    PageSize = 20
};

var result = await mediator.Send(query);
// result.Items - List<ProjectInstanceDto> for current page
// result.TotalItems - Total count across all pages
// result.CurrentPage - Calculated from Skip
// result.TotalPages - Calculated total pages
```

### Get All Items (Tree Views)
```csharp
var query = GetProjectInstancesQuery.AllItems();
var result = await mediator.Send(query);
// result.Items contains ALL items (no pagination)
```

### With Sorting and Search
```csharp
// Paginated with search and sorting
var query = new GetProjectInstancesQuery
{
    SearchTerm = "production",
    PageNumber = 2,
    PageSize = 50,
    OrderBy = "Name",
    OrderDirection = "asc"
};

// Using Skip (easier for DataGrid)
var query = new GetProjectInstancesQuery
{
    SearchTerm = "staging",
    Skip = 100,
    PageSize = 50,
    OrderBy = "Name",
    OrderDirection = "asc"
};

// All items with sorting (no search)
var query = GetProjectInstancesQuery.AllItems(
    orderBy: "CreatedAt",
    orderDirection: "desc"
);
```

## UI Components Usage

### Tree Views (Load All Data)
```csharp
// In component code
private GetProjectInstancesQuery _query = GetProjectInstancesQuery.AllItems();

private void HandleDataFetched(DataFetchedEventArgs<PaginatedList<ProjectInstanceDto>> eventArgs)
{
    AllInstances = eventArgs.Data.Items; // All items in one page
}
```

### Data Grid (Paginated with Search)
```csharp
private GetProjectInstancesQuery _query = new();
private string? _searchTerm;

private void OnLoadData(LoadDataArgs args)
{
    _query = new GetProjectInstancesQuery
    {
        SearchTerm = _searchTerm,  // Include search term
        Skip = args.Skip ?? 0,
        PageSize = args.Top ?? 20,
        OrderBy = ParseOrderBy(args.OrderBy),
        OrderDirection = ParseOrderDirection(args.OrderBy)
    };
    
    _refreshToken = Guid.NewGuid().ToString(); // Trigger refresh
}

private void OnSearchChanged()
{
    // Reset to first page when search changes
    _query = new GetProjectInstancesQuery
    {
        SearchTerm = _searchTerm,
        Skip = 0,
        PageSize = _pageSize
    };
    
    _refreshToken = Guid.NewGuid().ToString();
}
```

## Key Points

1. **One Query Class** - `GetProjectInstancesQuery` handles both paginated and non-paginated requests
2. **Helper Method** - Use `AllItems()` when you need all data (sets PageSize to int.MaxValue)
3. **Skip or PageNumber** - Use `Skip` for offset-based (DataGrid), or `PageNumber` for page-based pagination
4. **Auto-sync** - Setting `Skip` auto-calculates `PageNumber`, and vice versa
5. **Full-Text Search** - Set `SearchTerm` to search across Name, Description, MainHost, and Notes
6. **Always Returns PaginatedList** - Even when getting all items, you get a PaginatedList with all items in `Items`
7. **Tree Views** - Use `AllItems()` to load the complete hierarchy
8. **Data Grids** - Use `Skip` property directly from DataGrid args (simplest approach)
9. **Smart Caching** - Search results use separate cache keys
10. **Default Page Size** - 20 items per page

## Why This Design?

- **Simplicity**: One query class instead of two
- **Consistency**: Always returns `PaginatedList<T>`
- **Flexibility**: Easy to switch between paginated/non-paginated
- **Clear Intent**: `AllItems()` explicitly shows you want everything
- **Performance**: Data grid loads only what's needed, trees get full hierarchy
