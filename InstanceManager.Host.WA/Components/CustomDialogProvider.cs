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

        // Get OnLocationChanged method from base class using reflection
        var baseType = typeof(FluentDialogProvider);
        var locationChangedMethod = baseType.GetMethod("LocationChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Remove original subscription to avoid double handling
        if (locationChangedMethod != null)
        {
            var originalLocationChanged = (EventHandler<LocationChangedEventArgs>)Delegate.CreateDelegate(
                typeof(EventHandler<LocationChangedEventArgs>),
                this,
                locationChangedMethod
            );
            
            NavigationManager.LocationChanged -= originalLocationChanged;
        }

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
            DismissAll();
            StateHasChanged();
        }
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