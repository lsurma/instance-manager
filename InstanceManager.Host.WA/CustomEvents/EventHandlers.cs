using Microsoft.AspNetCore.Components;

namespace InstanceManager.Host.WA.CustomEvents;

[EventHandler("onWaSelectionChange", typeof(WebAwesomeSelectionChangeEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onWaExpand", typeof(WebAwesomeExpandEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onWaCollapse", typeof(WebAwesomeCollapseEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class EventHandlers
{
}

public class WebAwesomeSelectionChangeEventArgs : EventArgs
{
    public string? SelectedId { get; set; }
}

public class WebAwesomeExpandEventArgs : EventArgs
{
    public string? ItemId { get; set; }
}

public class WebAwesomeCollapseEventArgs : EventArgs
{
    public string? ItemId { get; set; }
}
