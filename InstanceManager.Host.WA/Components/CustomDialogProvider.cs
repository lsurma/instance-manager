using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;

namespace InstanceManager.Host.WA.Components;

public class CustomDialogProvider : FluentDialogProvider, IDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    
    private string? _previousPath;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Store initial path (without query string)
        _previousPath = GetPathFromUri(NavigationManager.Uri);
        
        // Subscribe to location changes
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var currentPath = GetPathFromUri(e.Location);
        
        // Only close dialogs if the path changed (not just query string)
        if (currentPath != _previousPath)
        {
            _previousPath = currentPath;
            // The base class will handle closing dialogs on StateHasChanged
            StateHasChanged();
        }
        
        // If only query string changed, do nothing (dialogs stay open)
    }

    private string GetPathFromUri(string uri)
    {
        var parsedUri = new Uri(uri);
        return parsedUri.GetLeftPart(UriPartial.Path);
    }

    public new void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        base.Dispose();
    }
}
