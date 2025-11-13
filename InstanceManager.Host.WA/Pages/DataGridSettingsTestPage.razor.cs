using System.Collections.Generic;
using System.Linq;
using InstanceManager.Host.WA.Components;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace InstanceManager.Host.WA.Pages
{
    public class DataGridTestItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public System.DateTime CreatedDate { get; set; }
    }

    public partial class DataGridSettingsTestPage : ComponentBase
    {
        private List<DataGridTestItem> _items = new();
        private int _totalItems = 100;
        private int _pageSize = 10;
        private List<DataGridTestItem> _allData = new();

        protected override void OnInitialized()
        {
            GenerateSampleData();
            _items = _allData.Take(_pageSize).ToList();
        }

        private void GenerateSampleData()
        {
            _allData = Enumerable.Range(1, _totalItems).Select(i => new DataGridTestItem
            {
                Id = i,
                Name = $"Item {i}",
                Description = $"This is the description for item {i}",
                CreatedDate = System.DateTime.Now.AddDays(-i)
            }).ToList();
        }

        private void OnLoadData(LoadDataArgs args)
        {
            var query = _allData.AsQueryable();

            if (!string.IsNullOrEmpty(args.OrderBy))
            {
                // Radzen's OrderBy is a string, so we can't use it directly with LINQ.
                // This is a simplified example. A real implementation would need a more robust solution.
                // For this test, we'll just handle paging.
            }

            _items = query.Skip(args.Skip.Value).Take(args.Top.Value).ToList();
            StateHasChanged();
        }
    }
}
