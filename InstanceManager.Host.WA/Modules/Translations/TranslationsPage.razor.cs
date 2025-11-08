using InstanceManager.Application.Contracts;
using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.DataSet;
using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Host.WA.Components;
using InstanceManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;

namespace InstanceManager.Host.WA.Modules.Translations;

public partial class TranslationsPage : ComponentBase, IDisposable
{
    [Inject] 
    private IDialogService DialogService { get; set; } = default!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    
    [Inject]
    private NavigationHelper NavHelper { get; set; } = default!;
    
    [Inject]
    private IRequestSender RequestSender { get; set; } = default!;
    
    private List<TranslationDto> AllTranslations { get; set; } = new();
    private List<DataSetDto> AllDataSets { get; set; } = new();
    private IDialogReference? _currentDialog;
    private string _refreshToken = Guid.NewGuid().ToString();
    private IList<TranslationDto> _selectedRows = new List<TranslationDto>();
    private GetTranslationsQuery _currentQuery = new GetTranslationsQuery
    {
        Pagination = new PaginationParameters { PageNumber = 1, PageSize = 15 }
    };
    
    // Should stay static - we dont wanna cache all different queries separately
    private string _cacheKey = "paginated_translations";
    
    private int _totalItems = 0;
    private int _pageSize = 20;
    private string? _searchTerm;
    private string? _cultureNameFilter;
    private Guid? _selectedTranslationId;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        LoadDataSetsAsync();
    }
    
    private async void LoadDataSetsAsync()
    {
        try
        {
            var result = await RequestSender.SendAsync<PaginatedList<DataSetDto>>(GetDataSetsQuery.AllItems());
            AllDataSets = result.Items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load data sets: {ex.Message}");
        }
    }
    
    private void HandleDataFetched(DataFetchedEventArgs<PaginatedList<TranslationDto>> eventArgs)
    {
        AllTranslations = eventArgs.Data.Items;
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
    
    private Task OnDataGridSelectionChanged(IList<TranslationDto> selectedRows)
    {
        _selectedRows = selectedRows;
        
        if (selectedRows != null && selectedRows.Count > 0)
        {
            var translation = selectedRows[0];
            _selectedTranslationId = translation.Id;

            NavigationManager.NavigateTo($"/translations?id={translation.Id}", false);
        }
        else
        {
            _selectedTranslationId = null;
        }
        
        return Task.CompletedTask;
    }
    
    private void RestoreDataGridSelection()
    {
        if (!_selectedTranslationId.HasValue)
        {
            _selectedRows = new List<TranslationDto>();
            return;
        }
        
        var selectedTranslation = AllTranslations.FirstOrDefault(t => t.Id == _selectedTranslationId.Value);
        
        if (selectedTranslation != null)
        {
            _selectedRows = new List<TranslationDto> { selectedTranslation };
        }
        else
        {
            _selectedRows = new List<TranslationDto>();
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
        
        var hasFiltersChanged = GetCurrentSearchTerm() != _searchTerm ||
                                !HasSameCultureFilter(_currentQuery.Filtering.QueryFilters);

        if (_currentQuery.Ordering.OrderBy != orderBy ||
            _currentQuery.Ordering.OrderDirection != orderDirection ||
            _currentQuery.Pagination.Skip != skip ||
            _currentQuery.Pagination.PageSize != pageSize ||
            hasFiltersChanged)
        {
            _currentQuery = new GetTranslationsQuery
            {
                Filtering = new FilteringParameters
                {
                    QueryFilters = BuildQueryFilters()
                },
                Ordering = new OrderingParameters { OrderBy = orderBy, OrderDirection = orderDirection },
                Pagination = new PaginationParameters { Skip = skip, PageSize = pageSize }
            };

            _refreshToken = Guid.NewGuid().ToString();
        }
    }

    private void OnSearchChanged()
    {
        _currentQuery = new GetTranslationsQuery
        {
            Filtering = new FilteringParameters
            {
                QueryFilters = BuildQueryFilters()
            },
            Pagination = new PaginationParameters { Skip = 0, PageSize = _pageSize }
        };

        _refreshToken = Guid.NewGuid().ToString();
    }

    private void OnCultureFilterChanged()
    {
        _currentQuery = new GetTranslationsQuery
        {
            Filtering = new FilteringParameters
            {
                QueryFilters = BuildQueryFilters()
            },
            Pagination = new PaginationParameters { Skip = 0, PageSize = _pageSize }
        };

        _refreshToken = Guid.NewGuid().ToString();
    }

    private List<IQueryFilter> BuildQueryFilters()
    {
        var filters = new List<IQueryFilter>();

        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            filters.Add(new SearchFilter { SearchTerm = _searchTerm });
        }

        if (!string.IsNullOrWhiteSpace(_cultureNameFilter))
        {
            filters.Add(new CultureNameFilter { Value = _cultureNameFilter });
        }

        return filters;
    }

    private string? GetCurrentSearchTerm()
    {
        return _currentQuery.Filtering.QueryFilters
            .OfType<SearchFilter>()
            .FirstOrDefault()?.SearchTerm;
    }
    
    private bool HasSameCultureFilter(List<IQueryFilter> filters)
    {
        var existingCultureFilter = filters.OfType<CultureNameFilter>().FirstOrDefault();
        
        if (string.IsNullOrWhiteSpace(_cultureNameFilter))
        {
            return existingCultureFilter == null;
        }
        
        return existingCultureFilter?.Value == _cultureNameFilter;
    }

    private void ClearCultureFilter()
    {
        _cultureNameFilter = null;
        OnCultureFilterChanged();
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
                _selectedTranslationId = null;
                await OpenTranslationPanelAsync(null);
            }
            else if (!string.IsNullOrEmpty(idParam) && Guid.TryParse(idParam, out var translationId))
            {
                _selectedTranslationId = translationId;
                var translation = AllTranslations.FirstOrDefault(t => t.Id == translationId);

                if (translation != null)
                {
                    await OpenTranslationPanelAsync(translation);
                }
            }
            else
            {
                _selectedTranslationId = null;
                
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
    
    private async Task OpenTranslationPanelAsync(TranslationDto? translation = null)
    {
        var isEditMode = translation != null;
        
        var parameters = new TranslationPanelParameters
        {
            Translation = isEditMode 
                ? translation! with { }
                : new TranslationDto
                {
                    Id = Guid.NewGuid(),
                    InternalGroupName = string.Empty,
                    ResourceName = string.Empty,
                    TranslationName = string.Empty,
                    CultureName = string.Empty,
                    Content = string.Empty,
                    CreatedAt = DateTimeOffset.UtcNow
                },
            
            IsEditMode = isEditMode,
            AvailableDataSets = AllDataSets,
            
            OnDataChanged = async () =>
            {
                _refreshToken = Guid.NewGuid().ToString();
                await InvokeAsync(StateHasChanged);
                return;
            }
        };

        var newDialog = await DialogService.ShowPanelAsync<TranslationPanel>(parameters, new DialogParameters
        {
            Title = isEditMode ? "Edit Translation" : "Create New Translation",
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
        
        if(result.Cancelled && currentId == translation?.Id.ToString())
        {
            NavigationManager.NavigateTo("/translations", false);
        }
        
        StateHasChanged();
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
