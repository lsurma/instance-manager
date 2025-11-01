# Pagination Implementation

## Overview
This document describes the pagination implementation added to the InstanceManager application.

## Changes Summary

### 1. New Model: PaginatedList<T>
**Location:** `InstanceManager.Application.Contracts/Common/PaginatedList.cs`

A generic response DTO for paginated results containing:
- `Items` - List of items in the current page
- `TotalItems` - Total number of items across all pages
- `PageSize` - Number of items per page
- `CurrentPage` - Current page number (1-based)
- `TotalPages` - Total number of pages
- `HasPreviousPage` - Boolean indicating if previous page exists
- `HasNextPage` - Boolean indicating if next page exists

### 2. Query: GetProjectInstancesQuery
**Location:** `InstanceManager.Application.Contracts/ProjectInstance/GetProjectInstancesQuery.cs`

Returns `PaginatedList<ProjectInstanceDto>` and includes:
- `OrderBy` - Property name to sort by
- `OrderDirection` - Sort direction ("asc" or "desc")
- `PageNumber` - Page number to retrieve (default: 1)
- `PageSize` - Number of items per page (default: 20)
- `AllItems()` - Static helper method to get all items without pagination (sets PageSize to int.MaxValue)

### 3. Handler: GetProjectInstancesQueryHandler
**Location:** `InstanceManager.Application.Core/ProjectInstance/Handlers/GetProjectInstancesQueryHandler.cs`

Handles the query and:
- Calculates total count before pagination
- Applies skip/take for pagination
- Returns `PaginatedList<ProjectInstanceDto>` with metadata

### 4. UI Component Updates: InstancesPage
**Locations:**
- `InstanceManager.Host.WA/Modules/Instances/InstancesPage.razor`
- `InstanceManager.Host.WA/Modules/Instances/InstancesPage.razor.cs`

Updated to use:
- **Single DataLoader** for all render modes (trees and grid)
- **Single callback** `HandleDataFetched` processes data for all views
- **Dynamic query switching:** Tree views use `GetProjectInstancesQuery.AllItems()`, grid uses paginated query
- **Dynamic cache keys:** Different cache keys for paginated vs non-paginated data
- Automatic query/cache updates when switching render modes
- Automatic query parameter updates when user changes pages or sorts data in grid

### 5. Home Page Update
**Location:** `InstanceManager.Host.WA/Pages/Home.razor`

Updated test page to handle `PaginatedList<ProjectInstanceDto>` response.

## Usage Examples

### Backend - Using Paginated Query
```csharp
var query = new GetProjectInstancesQuery
{
    PageNumber = 1,
    PageSize = 20,
    OrderBy = "Name",
    OrderDirection = "asc"
};

var result = await mediator.Send(query);
// result.Items - items on current page
// result.TotalItems - total count
// result.CurrentPage - current page number
// result.TotalPages - total pages
```

### Backend - Getting All Items (for trees)
```csharp
var query = GetProjectInstancesQuery.AllItems(orderBy: "Name", orderDirection: "asc");

var result = await mediator.Send(query);
// result.Items contains all items (PageSize is set to int.MaxValue)
// result.TotalItems - total count
// result.CurrentPage - will be 1
// result.TotalPages - will be 1
```

### Frontend - Radzen DataGrid
The DataGrid automatically handles pagination through the `LoadData` event:
```razor
<RadzenDataGrid Data="@AllInstances" 
                LoadData="@OnLoadData"
                Count="@_totalItems"
                AllowPaging="true" 
                PageSize="@_pageSize">
    <!-- columns -->
</RadzenDataGrid>
```

## Benefits

1. **Performance**: Only loads necessary data per page, reducing database load and network traffic
2. **Scalability**: Handles large datasets efficiently
3. **User Experience**: Faster page loads and responsive UI
4. **Flexibility**: Different views (tree vs grid) use appropriate data loading strategies
5. **Consistency**: Standardized pagination model can be reused across the application

## Future Enhancements

Consider adding:
- Filtering support in pagination queries
- Search capabilities with pagination
- Configurable page size options in UI
- Export functionality for full datasets
- Cursor-based pagination for real-time data
