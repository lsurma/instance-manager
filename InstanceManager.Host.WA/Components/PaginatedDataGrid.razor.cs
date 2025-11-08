using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;

namespace InstanceManager.Host.WA.Components;

/// <summary>
/// Reusable DataGrid component with built-in search, pagination, and sorting.
/// Wraps RadzenDataGrid with common functionality.
/// </summary>
/// <typeparam name="TItem">The type of items displayed in the grid</typeparam>
public partial class PaginatedDataGrid<TItem> : ComponentBase
{
    /// <summary>
    /// The list of items to display in the grid
    /// </summary>
    [Parameter]
    public List<TItem> Items { get; set; } = new();

    /// <summary>
    /// Total count of items (for pagination)
    /// </summary>
    [Parameter]
    public int TotalItems { get; set; }

    /// <summary>
    /// Page size for pagination
    /// </summary>
    [Parameter]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Placeholder text for the search box
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search...";

    /// <summary>
    /// Current search term
    /// </summary>
    [Parameter]
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Event callback when search term changes
    /// </summary>
    [Parameter]
    public EventCallback<string?> SearchTermChanged { get; set; }

    /// <summary>
    /// Event callback when data needs to be loaded (pagination, sorting, filtering)
    /// </summary>
    [Parameter]
    public EventCallback<LoadDataArgs> LoadData { get; set; }

    /// <summary>
    /// Event callback when search changes (triggers data reload)
    /// </summary>
    [Parameter]
    public EventCallback OnSearchChanged { get; set; }

    /// <summary>
    /// Selected rows
    /// </summary>
    [Parameter]
    public IList<TItem>? SelectedRows { get; set; }

    /// <summary>
    /// Event callback when selection changes
    /// </summary>
    [Parameter]
    public EventCallback<IList<TItem>> SelectedRowsChanged { get; set; }

    /// <summary>
    /// Columns definition
    /// </summary>
    [Parameter]
    public RenderFragment? Columns { get; set; }

    /// <summary>
    /// Additional filter controls to display next to search
    /// </summary>
    [Parameter]
    public RenderFragment? AdditionalFilters { get; set; }

    /// <summary>
    /// Allow filtering on the grid
    /// </summary>
    [Parameter]
    public bool AllowFiltering { get; set; } = true;

    /// <summary>
    /// Allow sorting on the grid
    /// </summary>
    [Parameter]
    public bool AllowSorting { get; set; } = true;

    /// <summary>
    /// Allow paging on the grid
    /// </summary>
    [Parameter]
    public bool AllowPaging { get; set; } = true;

    /// <summary>
    /// Selection mode for the grid
    /// </summary>
    [Parameter]
    public DataGridSelectionMode SelectionMode { get; set; } = DataGridSelectionMode.Single;

    private async Task OnLoadData(LoadDataArgs args)
    {
        if (LoadData.HasDelegate)
        {
            await LoadData.InvokeAsync(args);
        }
    }

    private async Task OnSelectionChanged(IList<TItem> selectedRows)
    {
        SelectedRows = selectedRows;

        if (SelectedRowsChanged.HasDelegate)
        {
            await SelectedRowsChanged.InvokeAsync(selectedRows);
        }
    }

    private void HandleSearchChanged()
    {
        if (SearchTermChanged.HasDelegate)
        {
            _ = SearchTermChanged.InvokeAsync(SearchTerm);
        }

        if (OnSearchChanged.HasDelegate)
        {
            _ = OnSearchChanged.InvokeAsync();
        }
    }

    private async Task HandleClearSearch()
    {
        SearchTerm = null;

        if (SearchTermChanged.HasDelegate)
        {
            await SearchTermChanged.InvokeAsync(SearchTerm);
        }

        if (OnSearchChanged.HasDelegate)
        {
            await OnSearchChanged.InvokeAsync();
        }
    }
}
