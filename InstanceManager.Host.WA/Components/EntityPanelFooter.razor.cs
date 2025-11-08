using Microsoft.AspNetCore.Components;

namespace InstanceManager.Host.WA.Components;

/// <summary>
/// Reusable footer component for entity create/edit panels.
/// Provides Save, Save and Close, Cancel, and Delete buttons with consistent behavior.
/// </summary>
public partial class EntityPanelFooter : ComponentBase
{
    /// <summary>
    /// Indicates whether the panel is in edit mode (true) or create mode (false)
    /// </summary>
    [Parameter]
    public bool IsEditMode { get; set; }

    /// <summary>
    /// Indicates whether a save operation is in progress
    /// </summary>
    [Parameter]
    public bool IsSaving { get; set; }

    /// <summary>
    /// Indicates whether a delete operation is in progress
    /// </summary>
    [Parameter]
    public bool IsDeleting { get; set; }

    /// <summary>
    /// Callback invoked when the Save button is clicked
    /// </summary>
    [Parameter]
    public EventCallback OnSave { get; set; }

    /// <summary>
    /// Callback invoked when the Save and Close button is clicked
    /// </summary>
    [Parameter]
    public EventCallback OnSaveAndClose { get; set; }

    /// <summary>
    /// Callback invoked when the Cancel button is clicked
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// Callback invoked when the Delete button is clicked
    /// </summary>
    [Parameter]
    public EventCallback OnDelete { get; set; }

    /// <summary>
    /// Whether to show the delete button (defaults to IsEditMode)
    /// </summary>
    [Parameter]
    public bool? ShowDeleteButton { get; set; }

    /// <summary>
    /// Text for the Save button in edit mode
    /// </summary>
    [Parameter]
    public string SaveButtonText { get; set; } = "Update";

    /// <summary>
    /// Text for the Create button in create mode
    /// </summary>
    [Parameter]
    public string CreateButtonText { get; set; } = "Create";

    /// <summary>
    /// Text for the Save and Close button in edit mode
    /// </summary>
    [Parameter]
    public string SaveAndCloseButtonText { get; set; } = "Update and Close";

    /// <summary>
    /// Text for the Create and Close button in create mode
    /// </summary>
    [Parameter]
    public string CreateAndCloseButtonText { get; set; } = "Create and Close";

    /// <summary>
    /// Text for the Cancel button
    /// </summary>
    [Parameter]
    public string CancelButtonText { get; set; } = "Cancel";

    /// <summary>
    /// Text for the Delete button
    /// </summary>
    [Parameter]
    public string DeleteButtonText { get; set; } = "Delete";

    /// <summary>
    /// Computed property to determine if delete button should be shown
    /// </summary>
    private bool ShouldShowDeleteButton => ShowDeleteButton ?? IsEditMode;

    private async Task HandleSaveClick()
    {
        if (OnSave.HasDelegate)
        {
            await OnSave.InvokeAsync();
        }
    }

    private async Task HandleSaveAndCloseClick()
    {
        if (OnSaveAndClose.HasDelegate)
        {
            await OnSaveAndClose.InvokeAsync();
        }
    }

    private async Task HandleCancelClick()
    {
        if (OnCancel.HasDelegate)
        {
            await OnCancel.InvokeAsync();
        }
    }

    private async Task HandleDeleteClick()
    {
        if (OnDelete.HasDelegate)
        {
            await OnDelete.InvokeAsync();
        }
    }
}
