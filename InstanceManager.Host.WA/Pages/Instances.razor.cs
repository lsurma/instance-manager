using InstanceManager.Application.Contracts;
using InstanceManager.Application.Contracts.ProjectInstance;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace InstanceManager.Host.WA.Pages;

public partial class Instances : ComponentBase
{
    [Inject]
    private IRequestSender RequestSender { get; set; } = default!;

    private List<ITreeViewItem> Items { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private string? ErrorMessage { get; set; }
    private ITreeViewItem? SelectedItem { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var query = new GetAllProjectInstancesQuery();
            var instances = await RequestSender.SendAsync(query);

            Items = BuildTreeItems(instances);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading instances: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private List<ITreeViewItem> BuildTreeItems(List<ProjectInstanceDto> instances)
    {
        // Get root items (items without parent)
        var rootItems = instances.Where(i => i.ParentProjectId == null).ToList();

        return rootItems.Select(instance => CreateTreeItem(instance, instances)).ToList();
    }

    private ITreeViewItem CreateTreeItem(ProjectInstanceDto instance, List<ProjectInstanceDto> allInstances)
    {
        var treeItem = new TreeViewItem
        {
            Text = instance.Name,
            Expanded = true,
            Id = instance.Id.ToString(),
            // You can add more metadata if needed
            // Value = instance.Id.ToString(),
        };

        // Get children for this instance
        var children = allInstances.Where(i => i.ParentProjectId == instance.Id).ToList();

        if (children.Any())
        {
            treeItem.Items = children.Select(child => CreateTreeItem(child, allInstances)).ToList();
        }

        return treeItem;
    }

    private void SelectedItemChanged(ITreeViewItem? item)
    {
        SelectedItem = item;

        if (item != null)
        {
            // Log or handle the selected item
            Console.WriteLine($"Selected item: {item.Text} (ID: {item.Id})");

            // You can add additional logic here, for example:
            // - Load details for the selected instance
            // - Navigate to a detail page
            // - Enable/disable action buttons
        }
        
        
    }
}