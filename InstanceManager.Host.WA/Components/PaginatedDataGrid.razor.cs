using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstanceManager.Host.WA.Components
{
    public partial class PaginatedDataGrid<TItem> : ComponentBase
    {
        private RadzenDataGrid<TItem> _dataGrid;
        private bool _isSettingsPanelOpen;
        private List<ColumnState> _columnStates = new();
        private bool _columnsInitialized;

        [Parameter]
        public List<TItem> Items { get; set; } = new();

        [Parameter]
        public int TotalItems { get; set; }

        [Parameter]
        public int PageSize { get; set; } = 20;

        [Parameter]
        public string SearchPlaceholder { get; set; } = "Search...";

        [Parameter]
        public string? SearchTerm { get; set; }

        [Parameter]
        public EventCallback<string?> SearchTermChanged { get; set; }

        [Parameter]
        public EventCallback<LoadDataArgs> LoadData { get; set; }

        [Parameter]
        public EventCallback OnSearchChanged { get; set; }

        [Parameter]
        public IList<TItem>? SelectedRows { get; set; }

        [Parameter]
        public EventCallback<IList<TItem>> SelectedRowsChanged { get; set; }

        [Parameter]
        public RenderFragment? Columns { get; set; }

        [Parameter]
        public RenderFragment? AdditionalFilters { get; set; }

        [Parameter]
        public bool AllowFiltering { get; set; } = true;

        [Parameter]
        public bool AllowSorting { get; set; } = true;

        [Parameter]
        public bool AllowPaging { get; set; } = true;

        [Parameter]
        public DataGridSelectionMode SelectionMode { get; set; } = DataGridSelectionMode.Single;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && _dataGrid.ColumnsCollection.Any() && !_columnsInitialized)
            {
                _columnStates = _dataGrid.ColumnsCollection.Select((col, index) => new ColumnState
                {
                    Title = col.Title,
                    PropertyName = col.Property,
                    Visible = col.Visible,
                    Order = index
                }).ToList();
                _columnsInitialized = true;
                await InvokeAsync(StateHasChanged);
            }
        }

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

        private async Task HandleSettingsChanged()
        {
            foreach (var columnState in _columnStates)
            {
                var column = _dataGrid.ColumnsCollection.FirstOrDefault(c => c.Property == columnState.PropertyName);
                if (column != null)
                {
                    column.Visible = columnState.Visible;
                    column.OrderIndex = columnState.Order;
                }
            }
            await _dataGrid.Reload();
        }
    }
}
