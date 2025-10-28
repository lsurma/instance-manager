using InstanceManager.Application.Contracts.ProjectInstance;
using InstanceManager.Host.WA.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace InstanceManager.Host.WA.Pages;

public partial class Instances : ComponentBase
{
    [Inject] 
    private IDialogService DialogService { get; set; } = default!;
    
    private List<ITreeViewItem> Items { get; set; } = new();
    private ITreeViewItem? SelectedItem { get; set; }
    private List<ProjectInstanceDto> AllInstances { get; set; } = new();
    private IDialogReference? _currentDialog;

    private void HandleDataFetched(List<ProjectInstanceDto> instances)
    {
        AllInstances = instances;
        
        // Get root instances (no parent)
        var rootInstances = instances.Where(i => i.ParentProjectId == null).ToList();

        // Update the tree in-place to preserve object references
        UpdateTreeItems(Items, rootInstances, instances);

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
        SelectedItem = item;

        if (item != null && Guid.TryParse(item.Id, out var instanceId))
        {
            // Find the instance from the all instances list
            var instance = AllInstances.FirstOrDefault(i => i.Id == instanceId);
            
            if (instance != null)
            {
                await OpenInstancePanelAsync(instance);
            }
        }
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
                : AllInstances
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
        
        if (!result.Cancelled)
        {
            // Refresh the data after saving
            StateHasChanged();
        }
    }
}
