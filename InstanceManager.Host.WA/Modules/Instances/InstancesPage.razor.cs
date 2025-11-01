using InstanceManager.Application.Contracts.ProjectInstance;
using InstanceManager.Host.WA.Components;
using InstanceManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace InstanceManager.Host.WA.Modules.Instances;

public partial class InstancesPage : ComponentBase, IDisposable
{
    [Inject] 
    private IDialogService DialogService { get; set; } = default!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    
    [Inject]
    private NavigationHelper NavHelper { get; set; } = default!;
    
    private List<ITreeViewItem> Items { get; set; } = new();
    private ITreeViewItem? SelectedItem { get; set; }
    private List<ProjectInstanceDto> AllInstances { get; set; } = new();
    private IDialogReference? _currentDialog;
    private string _refreshToken = Guid.NewGuid().ToString();
    private RenderMode _renderMode = RenderMode.WebAwesomeTree;
    private IList<ProjectInstanceDto> _selectedRows = new List<ProjectInstanceDto>();
    private bool _isGridInitialLoad = true;
    private GetAllProjectInstancesQuery _currentQuery = new();

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }
    
    private async void HandleDataFetched(DataFetchedEventArgs<List<ProjectInstanceDto>> eventArgs)
    {
        AllInstances = eventArgs.Data;
        
        // Log fetch information for debugging
        Console.WriteLine($"Data fetched - IsFromCache: {eventArgs.IsFromCache}, IsFirstFetch: {eventArgs.IsFirstFetch}");
        
        // Get root instances (no parent)
        var rootInstances = eventArgs.Data.Where(i => i.ParentProjectId == null).ToList();

        // Update the tree in-place to preserve object references
        UpdateTreeItems(Items, rootInstances, eventArgs.Data);
        
        // Process URL parameters on first live data fetch (e.g., direct link with ?id=xxx)
        if (eventArgs.IsFirstFetch && !eventArgs.IsFromCache)
        {
            await ProcessUrlParametersAsync();
        }

        StateHasChanged();
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

    private async void SelectedItemChanged(ITreeViewItem? item)
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
        
        return Task.CompletedTask;
    }
    
    private void OnLoadData(LoadDataArgs args)
    {
        _isGridInitialLoad = false;
        
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
        
        // Update query if parameters changed
        if (_currentQuery.OrderBy != orderBy || _currentQuery.OrderDirection != orderDirection)
        {
            _currentQuery = new GetAllProjectInstancesQuery
            {
                OrderBy = orderBy,
                OrderDirection = orderDirection
            };
            
            // Trigger data refresh
            _refreshToken = Guid.NewGuid().ToString();
        }
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
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var action = query["action"];
            var idParam = query["id"];
            
            if (action == "create")
            {
                SelectedItem = null;
                _selectedInstanceId = null;
                await OpenInstancePanelAsync(null);
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
                return;
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
