using InstanceManager.Application.Contracts;
using InstanceManager.Application.Contracts.ProjectInstance;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace InstanceManager.Host.WA.Components;

public partial class InstancePanel : IDialogContentComponent<InstancePanelParameters>
{
    [Parameter]
    public InstancePanelParameters Content { get; set; } = default!;
    
    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }
    
    [Inject] 
    private IRequestSender RequestSender { get; set; } = default!;
    
    [Inject]
    private IDialogService DialogService { get; set; } = default!;
    
    [Inject]
    private IToastService ToastService { get; set; } = default!;
    
    private bool IsSaving { get; set; }
    private bool IsDeleting { get; set; }
    private string? ErrorMessage { get; set; }
    
    private List<Option<Guid?>> ParentSelectItems
    {
        get
        {
            var items = new List<Option<Guid?>>
            {
                new Option<Guid?> { Value = null, Text = "-- No Parent --" }
            };
            
            if (Content?.AvailableParentInstances != null)
            {
                items.AddRange(Content.AvailableParentInstances
                    .Where(p => Content.Instance == null || p.Id != Content.Instance.Id) // Exclude self
                    .Select(p => new Option<Guid?> 
                    { 
                        Value = p.Id, 
                        Text = p.Name 
                    }));
            }
            
            return items;
        }
    }
    
    private async Task HandleSubmitAsync(bool closeAfterSave = true)
    {
        if (Content?.Instance == null) return;
        
        try
        {
            IsSaving = true;
            ErrorMessage = null;
            
            // Place for insert/update logic
            await RequestSender.SendAsync(new SaveProjectInstanceCommand()
            {
                Id = Content.IsEditMode ? Content.Instance.Id : null,
                Name = Content.Instance.Name,
                Description = Content.Instance.Description,
                MainHost = Content.Instance.MainHost,
                Notes = Content.Instance.Notes,
                ParentProjectId = Content.Instance.ParentProjectId
            });
            
            // Show success toast
            ToastService.ShowSuccess($"Instance '{Content.Instance.Name}' {(Content.IsEditMode ? "updated" : "created")} successfully");
            
            // Notify parent component that data changed
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            // Close dialog with success result if requested
            if (closeAfterSave)
            {
                await Dialog!.CloseAsync(Content.Instance);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to {(Content.IsEditMode ? "update" : "create")} instance: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }
    
    private async Task HandleCancelAsync()
    {
        await Dialog!.CancelAsync();
    }
    
    private async Task HandleDeleteClickAsync()
    {
        if (Content?.Instance == null) return;
        
        var dialog = await DialogService.ShowConfirmationAsync(
            $"Are you sure you want to delete '{Content.Instance.Name}'?",
            "Yes",
            "No",
            "Confirm");
        
        var result = await dialog.Result;
        
        if (result.Cancelled)
        {
            return;
        }
        
        try
        {
            IsDeleting = true;
            ErrorMessage = null;

            var command = new DeleteProjectInstanceCommand(Content.Instance.Id);
            await RequestSender.SendAsync(command);
            
            // Show success toast
            ToastService.ShowSuccess($"Instance '{Content.Instance.Name}' deleted successfully");
            
            // Notify parent component that data changed
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            // Close dialog with the deleted instance to signal deletion
            await Dialog!.CloseAsync(Content.Instance);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete instance: {ex.Message}";
        }
        finally
        {
            IsDeleting = false;
        }
    }
}

public class InstancePanelParameters
{
    public ProjectInstanceDto Instance { get; set; } = default!;
    public bool IsEditMode { get; set; }
    public List<ProjectInstanceDto> AvailableParentInstances { get; set; } = new();
    public Func<Task>? OnDataChanged { get; set; }
}
