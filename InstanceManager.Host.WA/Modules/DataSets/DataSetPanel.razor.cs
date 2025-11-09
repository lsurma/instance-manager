using InstanceManager.Application.Contracts;
using InstanceManager.Application.Contracts.Modules.DataSet;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;

namespace InstanceManager.Host.WA.Modules.DataSets;

public partial class DataSetPanel : IDialogContentComponent<DataSetPanelParameters>
{
    [Parameter]
    public DataSetPanelParameters Content { get; set; } = default!;
    
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
    private HashSet<Guid> SelectedIncludeIds { get; set; } = new();
    private string AllowedIdentityIdsText { get; set; } = string.Empty;

    protected override Task OnInitializedAsync()
    {
        // Initialize selected includes from the DataSet
        if (Content?.DataSet?.IncludedDataSetIds != null)
        {
            SelectedIncludeIds = new HashSet<Guid>(Content.DataSet.IncludedDataSetIds);
        }

        // Initialize AllowedIdentityIds text from DataSet
        if (Content?.DataSet?.AllowedIdentityIds != null && Content.DataSet.AllowedIdentityIds.Any())
        {
            AllowedIdentityIdsText = string.Join(Environment.NewLine, Content.DataSet.AllowedIdentityIds);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Parses the AllowedIdentityIdsText into a list of trimmed, non-empty identity IDs.
    /// Supports newlines, semicolons, and commas as separators.
    /// </summary>
    private List<string> ParseAllowedIdentityIds()
    {
        if (string.IsNullOrWhiteSpace(AllowedIdentityIdsText))
        {
            return new List<string>();
        }

        var separators = new[] { '\n', '\r', ';', ',' };

        return AllowedIdentityIdsText
            .Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct()
            .ToList();
    }
    
    private void HandleIncludeChanged(Guid dataSetId, bool isSelected)
    {
        if (isSelected)
        {
            SelectedIncludeIds.Add(dataSetId);
        }
        else
        {
            SelectedIncludeIds.Remove(dataSetId);
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
        if (Content?.DataSet == null) return;
        
        try
        {
            IsSaving = true;
            ErrorMessage = null;
            
            await RequestSender.SendAsync(new SaveDataSetCommand()
            {
                Id = Content.IsEditMode ? Content.DataSet.Id : null,
                Name = Content.DataSet.Name,
                Description = Content.DataSet.Description,
                Notes = Content.DataSet.Notes,
                AllowedIdentityIds = ParseAllowedIdentityIds(),
                IncludedDataSetIds = SelectedIncludeIds.ToList()
            });
            
            ToastService.ShowSuccess($"Data Set '{Content.DataSet.Name}' {(Content.IsEditMode ? "updated" : "created")} successfully");
            
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            if (closeAfterSave)
            {
                await Dialog!.CloseAsync(DialogResult.Cancel(Content.DataSet));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to {(Content.IsEditMode ? "update" : "create")} data set: {ex.Message}";
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
        if (Content?.DataSet == null) return;
        
        var dialog = await DialogService.ShowConfirmationAsync(
            $"Are you sure you want to delete '{Content.DataSet.Name}'?",
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

            var command = new DeleteDataSetCommand { Id = Content.DataSet.Id };
            await RequestSender.SendAsync(command);
            
            ToastService.ShowSuccess($"Data Set '{Content.DataSet.Name}' deleted successfully");
            
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            await Dialog!.CloseAsync(Content.DataSet);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete data set: {ex.Message}";
        }
        finally
        {
            IsDeleting = false;
        }
    }
}

public class DataSetPanelParameters
{
    public DataSetDto DataSet { get; set; } = default!;
    public bool IsEditMode { get; set; }
    public List<DataSetDto> AvailableDataSets { get; set; } = new();
    public Func<Task>? OnDataChanged { get; set; }
}
