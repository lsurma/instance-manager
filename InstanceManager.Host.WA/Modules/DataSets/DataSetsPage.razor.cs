using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.DataSet;
using InstanceManager.Host.WA.Components;
using InstanceManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace InstanceManager.Host.WA.Modules.DataSets;

public partial class DataSetsPage : ComponentBase, IDisposable
{
    [Inject] 
    private IDialogService DialogService { get; set; } = default!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    
    [Inject]
    private NavigationHelper NavHelper { get; set; } = default!;
    
    private List<DataSetDto> AllDataSets { get; set; } = new();
    private IDialogReference? _currentDialog;
    private string _refreshToken = Guid.NewGuid().ToString();
    private IList<DataSetDto> _selectedRows = new List<DataSetDto>();
    private GetDataSetsQuery _currentQuery = new GetDataSetsQuery
    {
        Pagination = new PaginationParameters { PageNumber = 1, PageSize = 15 }
    };
    private string _cacheKey = "paginated_datasets";
    private int _totalItems = 0;
    private int _pageSize = 20;
    private string? _searchTerm;
    private Guid? _selectedDataSetId;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }
    
    private void HandleDataFetched(DataFetchedEventArgs<PaginatedList<DataSetDto>> eventArgs)
    {
        AllDataSets = eventArgs.Data.Items;
        _totalItems = eventArgs.Data.TotalItems;
        _pageSize = eventArgs.Data.PageSize;
        
        Console.WriteLine($"Data fetched - IsFromCache: {eventArgs.IsFromCache}, IsFirstFetch: {eventArgs.IsFirstFetch}, Total: {_totalItems}, Page: {eventArgs.Data.CurrentPage}");
        
        RestoreDataGridSelection();
        
        // Process URL parameters on first live data fetch
        if (eventArgs.IsFirstFetch && !eventArgs.IsFromCache)
        {
            _ = ProcessUrlParametersAsync();
        }

        StateHasChanged();
    }
    
    private Task OnDataGridSelectionChanged(IList<DataSetDto> selectedRows)
    {
        _selectedRows = selectedRows;
        
        if (selectedRows != null && selectedRows.Count > 0)
        {
            var dataSet = selectedRows[0];
            _selectedDataSetId = dataSet.Id;

            NavigationManager.NavigateTo($"/datasets?id={dataSet.Id}", false);
        }
        else
        {
            _selectedDataSetId = null;
        }
        
        return Task.CompletedTask;
    }
    
    private void RestoreDataGridSelection()
    {
        if (!_selectedDataSetId.HasValue)
        {
            _selectedRows = new List<DataSetDto>();
            return;
        }
        
        var selectedDataSet = AllDataSets.FirstOrDefault(i => i.Id == _selectedDataSetId.Value);
        
        if (selectedDataSet != null)
        {
            _selectedRows = new List<DataSetDto> { selectedDataSet };
        }
        else
        {
            _selectedRows = new List<DataSetDto>();
        }
    }
    
    private void OnLoadData(LoadDataArgs args)
    {
        string? orderBy = null;
        string? orderDirection = null;
        
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            var orderByParts = args.OrderBy.Split(' ');
            orderBy = orderByParts[0];
            orderDirection = orderByParts.Length > 1 && orderByParts[1].ToLower() == "desc" ? "desc" : "asc";
        }
        
        var skip = args.Skip ?? 0;
        var pageSize = args.Top ?? 20;
        
        if (_currentQuery.Ordering.OrderBy != orderBy || 
            _currentQuery.Ordering.OrderDirection != orderDirection ||
            _currentQuery.Pagination.Skip != skip ||
            _currentQuery.Pagination.PageSize != pageSize ||
            _currentQuery.Filtering.SearchTerm != _searchTerm)
        {
            _currentQuery = new GetDataSetsQuery
            {
                Filtering = new FilteringParameters { SearchTerm = _searchTerm },
                Ordering = new OrderingParameters { OrderBy = orderBy, OrderDirection = orderDirection },
                Pagination = new PaginationParameters { Skip = skip, PageSize = pageSize }
            };
            
            _cacheKey = string.IsNullOrWhiteSpace(_searchTerm) 
                ? "paginated_datasets" 
                : $"search_{_searchTerm}_datasets_paginated";
            
            _refreshToken = Guid.NewGuid().ToString();
        }
    }
    
    private void OnSearchChanged()
    {
        _currentQuery = new GetDataSetsQuery
        {
            Filtering = new FilteringParameters { SearchTerm = _searchTerm },
            Pagination = new PaginationParameters { Skip = 0, PageSize = _pageSize }
        };
        
        _cacheKey = string.IsNullOrWhiteSpace(_searchTerm) 
            ? "paginated_datasets" 
            : $"search_{_searchTerm}_datasets_paginated";
        
        _refreshToken = Guid.NewGuid().ToString();
    }
    
    private void ClearSearch()
    {
        _searchTerm = null;
        OnSearchChanged();
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
                _selectedDataSetId = null;
                await OpenDataSetPanelAsync(null);
            }
            else if (!string.IsNullOrEmpty(idParam) && Guid.TryParse(idParam, out var dataSetId))
            {
                _selectedDataSetId = dataSetId;
                var dataSet = AllDataSets.FirstOrDefault(i => i.Id == dataSetId);

                if (dataSet != null)
                {
                    await OpenDataSetPanelAsync(dataSet);
                }
            }
            else
            {
                _selectedDataSetId = null;
                
                if (_currentDialog != null)
                {
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
    
    private async Task OpenDataSetPanelAsync(DataSetDto? dataSet = null)
    {
        var isEditMode = dataSet != null;
        
        var parameters = new DataSetPanelParameters
        {
            DataSet = isEditMode 
                ? dataSet! with { }
                : new DataSetDto
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow
                },
            
            IsEditMode = isEditMode,
            
            AvailableDataSets = isEditMode 
                ? AllDataSets.Where(i => i.Id != dataSet!.Id).ToList()
                : AllDataSets,
            
            OnDataChanged = async () =>
            {
                _refreshToken = Guid.NewGuid().ToString();
                await InvokeAsync(StateHasChanged);
                return;
            }
        };

        var newDialog = await DialogService.ShowPanelAsync<DataSetPanel>(parameters, new DialogParameters
        {
            Title = isEditMode ? "Edit Data Set" : "Create New Data Set",
            Width = "600px",
            TrapFocus = false,
            Modal = false,
            Id = $"panel-{Guid.NewGuid()}"
        });
        
        if (_currentDialog != null)
        {
            await _currentDialog.CloseAsync();
        }
        
        _currentDialog = newDialog;

        var result = await _currentDialog.Result;
        _currentDialog = null;
        var currentId = NavHelper.GetQueryParameter("id");
        
        if(result.Cancelled && currentId == dataSet?.Id.ToString())
        {
            NavigationManager.NavigateTo("/datasets", false);
        }
        
        StateHasChanged();
    }
    
    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
