using Microsoft.AspNetCore.Components;

namespace InstanceManager.Host.WA.CustomEvents;

[EventHandler("onWaSelectionChange", typeof(WebAwesomeSelectionChangeEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class EventHandlers
{
}