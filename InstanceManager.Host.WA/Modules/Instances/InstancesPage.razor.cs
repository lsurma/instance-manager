using System.Web;
using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.ProjectInstance;
using InstanceManager.Host.WA.Components;
using InstanceManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;

namespace InstanceManager.Host.WA.Modules.Instances;

public partial class InstancesPage : ComponentBase, IDisposable
{
    [Inject] 
    private IDialogService DialogService { get; set; } = null!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    
    [Inject]
    private NavigationHelper NavHelper { get; set; } = null!;
    
    private List<ITreeViewItem> Items { get; set; } = new();
    private ITreeViewItem? SelectedItem { get; set; }
    private List<ProjectInstanceDto> AllInstances { get; set; } = new();
    private IDialogReference? _currentDialog;
    private string _refreshToken = Guid.NewGuid().ToString();
    private RenderMode _renderMode = RenderMode.WebAwesomeTree;
    private IList<ProjectInstanceDto> _selectedRows = new List<ProjectInstanceDto>();
    private GetProjectInstancesQuery _currentQuery = GetProjectInstancesQuery.AllItems();

    // Should stay static - we dont wanna cache all different queries separately
    private string _cacheKey = "all_project_instances";

    private int _totalItems;
    private int _pageSize = 20;
    private string? _searchTerm;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        UpdateQueryForRenderMode();
    }
    
    private void HandleDataFetched(DataFetchedEventArgs<PaginatedList<ProjectInstanceDto>> eventArgs)
    {
        AllInstances = eventArgs.Data.Items;
        _totalItems = eventArgs.Data.TotalItems;
        _pageSize = eventArgs.Data.PageSize;
        
        // Log fetch information for debugging
        Console.WriteLine($"Data fetched - IsFromCache: {eventArgs.IsFromCache}, IsFirstFetch: {eventArgs.IsFirstFetch}, Total: {_totalItems}, Page: {eventArgs.Data.CurrentPage}");
        
        // Get root instances (no parent) for tree views
        var rootInstances = eventArgs.Data.Items.Where(i => i.ParentProjectId == null).ToList();

        // Update the tree in-place to preserve object references
        UpdateTreeItems(Items, rootInstances, eventArgs.Data.Items);
        
        // Restore DataGrid selection if in DataGrid mode
        if (_renderMode == RenderMode.DataGrid && _selectedInstanceId.HasValue)
        {
            RestoreDataGridSelection();
        }
        
        // Process URL parameters on first live data fetch (e.g., direct link with ?id=xxx)
        if (eventArgs.IsFirstFetch && !eventArgs.IsFromCache)
        {
            _ = ProcessUrlParametersAsync();
        }

        StateHasChanged();
    }

    private void SwitchRenderMode(RenderMode newMode)
    {
        if (_renderMode == newMode)
            return;
            
        _renderMode = newMode;
        UpdateQueryForRenderMode();
        _refreshToken = Guid.NewGuid().ToString();
    }
    
    private void UpdateQueryForRenderMode()
    {
        if (_renderMode == RenderMode.DataGrid)
        {
            // DataGrid uses pagination
            _currentQuery = new GetProjectInstancesQuery
            {
                Pagination = new PaginationParameters { PageNumber = 1, PageSize = 15 }
            };
            _cacheKey = "paginated_project_instances";
        }
        else
        {
            // Tree views need all items
            _currentQuery = GetProjectInstancesQuery.AllItems();
            _cacheKey = "all_project_instances";
        }
    }
    
    private void UpdateTreeItems(List<ITreeViewItem> treeItems, List<ProjectInstanceDto> currentLevelInstances, List<ProjectInstanceDto> allInstances)
    {
        // Get current IDs in the tree
        var currentIds = treeItems.Select(i => i.Id).ToList();
        var newIds = currentLevelInstances.Select(i => i.Id.ToString()).ToList();

        // Remove items that no longer exist
        var itemsToRemove = treeItems.Where(item => !newIds.Contains(item.Id)).ToList();
        foreach (var item in itemsToRemove)
        {
            treeItems.Remove(item);
        }

        // Update or add items
        foreach (var instance in currentLevelInstances)
        {
            var instanceId = instance.Id.ToString();
            var existingItem = treeItems.FirstOrDefault(i => i.Id == instanceId) as TreeViewItem;

            if (existingItem == null)
            {
                // Create new item
                var newItem = new TreeViewItem
                {
                    Id = instanceId,
                    Text = instance.Name,
                    Expanded = true,
                    Items = new List<ITreeViewItem>()
                };
                treeItems.Add(newItem);
                existingItem = newItem;
            }
            else
            {
                // Update existing item properties
                existingItem.Text = instance.Name;

                // Ensure Items collection exists
                if (existingItem.Items == null)
                {
                    existingItem.Items = new List<ITreeViewItem>();
                }
            }

            // Get children for this instance
            var childInstances = allInstances.Where(i => i.ParentProjectId == instance.Id).ToList();

            // Recursively update children
            var itemsList = existingItem.Items as List<ITreeViewItem> ?? existingItem.Items?.ToList() ?? new List<ITreeViewItem>();
            UpdateTreeItems(itemsList, childInstances, allInstances);
            existingItem.Items = itemsList;
        }
    }

    private void SelectedItemChanged(ITreeViewItem? item)
    {
        // Only process if FluentTree is active
        if (_renderMode != RenderMode.FluentTree)
        {
            return;
        }

        var idInUrl = NavHelper.GetQueryParameter("id");

        if (item != null && Guid.TryParse(item.Id, out var instanceId))
        {
            // Update URL with instance ID
            NavigationManager.NavigateTo($"/instances?id={instanceId}", false);
        }
        else if (Guid.TryParse(idInUrl, out _))
        {
            // Clear URL parameters
            NavigationManager.NavigateTo("/instances", false);
        }
    }
    
    private void HandleWebAwesomeItemSelected(Guid instanceId)
    {
        _selectedInstanceId = instanceId;
        // Update URL with instance ID
        NavigationManager.NavigateTo($"/instances?id={instanceId}", false);
    }
    
    private Task OnDataGridSelectionChanged(IList<ProjectInstanceDto> selectedRows)
    {
        if (_renderMode != RenderMode.DataGrid)
        {
            return Task.CompletedTask;
        }
        
        _selectedRows = selectedRows;
        
        if (selectedRows != null && selectedRows.Count > 0)
        {
            var instance = selectedRows[0];
            _selectedInstanceId = instance.Id;

            NavigationManager.NavigateTo($"/instances?id={instance.Id}", false);
        }
        else
        {
            _selectedInstanceId = null;
        }
        
        return Task.CompletedTask;
    }
    
    private void RestoreDataGridSelection()
    {
        if (!_selectedInstanceId.HasValue)
        {
            _selectedRows = new List<ProjectInstanceDto>();
            return;
        }
        
        // Find the previously selected instance in the new data
        var selectedInstance = AllInstances.FirstOrDefault(i => i.Id == _selectedInstanceId.Value);
        
        if (selectedInstance != null)
        {
            // Restore selection
            _selectedRows = new List<ProjectInstanceDto> { selectedInstance };
        }
        else
        {
            // Selected instance not in current page, clear selection
            _selectedRows = new List<ProjectInstanceDto>();
        }
    }
    
    private void OnLoadData(LoadDataArgs args)
    {
        // Parse OrderBy from args
        string? orderBy = null;
        string? orderDirection = null;
        
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            // Parse OrderBy string (e.g., "Name" or "Name desc")
            var orderByParts = args.OrderBy.Split(' ');
            orderBy = orderByParts[0];
            orderDirection = orderByParts.Length > 1 && orderByParts[1].ToLower() == "desc" ? "desc" : "asc";
        }
        
        var skip = args.Skip ?? 0;
        var pageSize = args.Top ?? 20;
        
        // Update query if parameters changed
        if (_currentQuery.Ordering.OrderBy != orderBy ||
            _currentQuery.Ordering.OrderDirection != orderDirection ||
            _currentQuery.Pagination.Skip != skip ||
            _currentQuery.Pagination.PageSize != pageSize ||
            GetCurrentSearchTerm() != _searchTerm)
        {
            _currentQuery = new GetProjectInstancesQuery
            {
                Filtering = BuildFilteringParameters(),
                Ordering = new OrderingParameters { OrderBy = orderBy, OrderDirection = orderDirection },
                Pagination = new PaginationParameters { Skip = skip, PageSize = pageSize }
            };

            // Trigger data refresh
            _refreshToken = Guid.NewGuid().ToString();
        }
    }

    private void OnSearchChanged()
    {
        // Reset to first page when search changes
        _currentQuery = new GetProjectInstancesQuery
        {
            Filtering = BuildFilteringParameters(),
            Pagination = new PaginationParameters { Skip = 0, PageSize = _pageSize }
        };

        _refreshToken = Guid.NewGuid().ToString();
    }

    private FilteringParameters BuildFilteringParameters()
    {
        var filters = new List<IQueryFilter>();

        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            filters.Add(new SearchFilter { SearchTerm = _searchTerm });
        }

        return new FilteringParameters { QueryFilters = filters };
    }

    private string? GetCurrentSearchTerm()
    {
        return _currentQuery.Filtering.QueryFilters
            .OfType<SearchFilter>()
            .FirstOrDefault()?.SearchTerm;
    }

    private Guid? _selectedInstanceId;
    
    private Guid? GetSelectedInstanceId()
    {
        if (_selectedInstanceId.HasValue)
        {
            return _selectedInstanceId;
        }
        
        if (SelectedItem != null && Guid.TryParse(SelectedItem.Id, out var instanceId))
        {
            return instanceId;
        }
        return null;
    }
    
    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        await ProcessUrlParametersAsync();
    }
    
    private async Task ProcessUrlParametersAsync()
    {
        try
        {
            var uri = new Uri(NavigationManager.Uri);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var action = query["action"];
            var idParam = query["id"];
            
            if (action == "create")
            {
                SelectedItem = null;
                _selectedInstanceId = null;
                await OpenInstancePanelAsync();
            }
            else if (!string.IsNullOrEmpty(idParam) && Guid.TryParse(idParam, out var instanceId))
            {
                _selectedInstanceId = instanceId;
                var instance = AllInstances.FirstOrDefault(i => i.Id == instanceId);

                if (instance != null)
                {
                    SelectedItem = Items.FirstOrDefault(i => i.Id == instanceId.ToString());
                    await OpenInstancePanelAsync(instance);
                }
            }
            else
            {
                SelectedItem = null;
                _selectedInstanceId = null;
                
                 if (_currentDialog != null)
                 {
                    // No query params, close any open dialog
                    await _currentDialog.CloseAsync();
                    _currentDialog = null;
                 }
            }
        }
        catch
        {
            
        }
        
        StateHasChanged();
    }
    
    private async Task OpenInstancePanelAsync(ProjectInstanceDto? instance = null)
    {
        var isEditMode = instance != null;
        
        // Clear tree selection when creating a new instance
        if (!isEditMode)
        {
            SelectedItem = null;
        }
        
        var parameters = new InstancePanelParameters
        {
            Instance = isEditMode 
                ? instance! with { } // Create a copy to avoid modifying the original
                : new ProjectInstanceDto
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow
                },
            
            IsEditMode = isEditMode,
            
            AvailableParentInstances = isEditMode 
                ? AllInstances.Where(i => i.Id != instance!.Id).ToList()
                : AllInstances,
            
            OnDataChanged = async () =>
            {
                _refreshToken = Guid.NewGuid().ToString();
                await InvokeAsync(StateHasChanged);
            }
        };

        var newDialog = await DialogService.ShowPanelAsync<InstancePanel>(parameters, new DialogParameters
        {
            Title = isEditMode ? "Edit Instance" : "Create New Instance",
            Width = "600px",
            TrapFocus = false,
            Modal = false,
            Id = $"panel-{Guid.NewGuid()}"
        });
        
        // Close the previous dialog after opening the new one to avoid flickering
        if (_currentDialog != null)
        {
            await _currentDialog.CloseAsync();
        }
        
        _currentDialog = newDialog;

        var result = await _currentDialog.Result;
        _currentDialog = null;
        var currentId = NavHelper.GetQueryParameter("id");
        
        if(result.Cancelled && currentId == instance?.Id.ToString())
        {
            // Clear URL parameters after closing the panel
            NavigationManager.NavigateTo("/instances", false);
        }
        
        // Refresh the data after saving
        StateHasChanged();
    }
    
    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}

public enum RenderMode
{
    FluentTree,
    WebAwesomeTree,
    DataGrid
}
