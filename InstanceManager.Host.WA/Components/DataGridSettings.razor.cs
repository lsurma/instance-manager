using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.FluentUI.AspNetCore.Components;

namespace InstanceManager.Host.WA.Components
{
    public partial class DataGridSettings : ComponentBase
    {
        [Parameter]
        public bool IsOpen { get; set; }

        [Parameter]
        public EventCallback<bool> IsOpenChanged { get; set; }

        [Parameter]
        public List<ColumnState> Columns { get; set; } = new List<ColumnState>();

        [Parameter]
        public EventCallback OnSettingsChanged { get; set; }

        private async Task HandleDismiss()
        {
            await IsOpenChanged.InvokeAsync(false);
        }

        private async Task HandleColumnOrderUpdate(FluentSortableListEventArgs args)
        {
            var movedItem = Columns[args.OldIndex];
            Columns.RemoveAt(args.OldIndex);
            Columns.Insert(args.NewIndex, movedItem);

            for (int i = 0; i < Columns.Count; i++)
            {
                Columns[i].Order = i;
            }
            await NotifyChanges();
        }

        private async Task NotifyChanges()
        {
            if (OnSettingsChanged.HasDelegate)
            {
                await OnSettingsChanged.InvokeAsync();
            }
        }
    }
}
