using InstanceManager.Application.Contracts;
using InstanceManager.Application.Contracts.Modules.DataSet;
using InstanceManager.Application.Contracts.Modules.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;

namespace InstanceManager.Host.WA.Modules.Translations;

public partial class TranslationPanel : IDialogContentComponent<TranslationPanelParameters>
{
    [Parameter]
    public TranslationPanelParameters Content { get; set; } = default!;
    
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
    
    private List<Option<Guid?>> DataSetSelectItems
    {
        get
        {
            var items = new List<Option<Guid?>>
            {
                new Option<Guid?> { Value = null, Text = "-- None --" }
            };
            
            if (Content?.AvailableDataSets != null)
            {
                items.AddRange(Content.AvailableDataSets
                    .Select(ds => new Option<Guid?> 
                    { 
                        Value = ds.Id, 
                        Text = ds.Name 
                    }));
            }
            
            return items;
        }
    }
    
    private async Task HandleKeyDownAsync(FluentKeyCodeEventArgs args)
    {
        // Ctrl+S to save
        if (args.CtrlKey && args.Key == KeyCode.KeyS)
        {
            await HandleSubmitAsync(closeAfterSave: false);
        }
    }
    
    private async Task HandleSubmitAsync(bool closeAfterSave = true)
    {
        if (Content?.Translation == null) return;
        
        try
        {
            IsSaving = true;
            ErrorMessage = null;
            
            await RequestSender.SendAsync(new SaveTranslationCommand
            {
                Id = Content.IsEditMode ? Content.Translation.Id : null,
                InternalGroupName = Content.Translation.InternalGroupName,
                ResourceName = Content.Translation.ResourceName,
                TranslationName = Content.Translation.TranslationName,
                CultureName = Content.Translation.CultureName,
                Content = Content.Translation.Content,
                DataSetId = Content.Translation.DataSetId
            });
            
            ToastService.ShowSuccess($"Translation '{Content.Translation.TranslationName}' {(Content.IsEditMode ? "updated" : "created")} successfully");
            
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            if (closeAfterSave)
            {
                await Dialog!.CloseAsync(DialogResult.Cancel(Content.Translation));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to {(Content.IsEditMode ? "update" : "create")} translation: {ex.Message}";
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
        if (Content?.Translation == null) return;
        
        var dialog = await DialogService.ShowConfirmationAsync(
            $"Are you sure you want to delete translation '{Content.Translation.TranslationName}'?",
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

            var command = new DeleteTranslationCommand { Id = Content.Translation.Id };
            await RequestSender.SendAsync(command);
            
            ToastService.ShowSuccess($"Translation '{Content.Translation.TranslationName}' deleted successfully");
            
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            await Dialog!.CloseAsync(Content.Translation);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete translation: {ex.Message}";
        }
        finally
        {
            IsDeleting = false;
        }
    }
}

public class TranslationPanelParameters
{
    public TranslationDto Translation { get; set; } = default!;
    public bool IsEditMode { get; set; }
    public List<DataSetDto> AvailableDataSets { get; set; } = new();
    public Func<Task>? OnDataChanged { get; set; }
}
