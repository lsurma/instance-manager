export function afterStarted(blazor) {
    console.log(blazor);
    blazor.registerCustomEventType('WaSelectionChange', {
        browserEventName: 'wa-selection-change',
        createEventArgs: event => {
            // Get the selected items from event.detail.selection
            const selectedItems = event.detail?.selection || [];
            
            // Get the first selected item's data-id
            let selectedId = null;
            if (selectedItems.length > 0) {
                selectedId = selectedItems[0].getAttribute('data-id');
            }
            
            return {
                selectedId: selectedId
            };
        }
    });
    
    blazor.registerCustomEventType('WaExpand', {
        browserEventName: 'wa-expand',
        createEventArgs: event => {
            const target = event.target;
            let itemId = target.getAttribute('data-id');
            
            return {
                itemId: itemId
            };
        }
    });
    
    blazor.registerCustomEventType('WaCollapse', {
        browserEventName: 'wa-collapse',
        createEventArgs: event => {
            const target = event.target;
            let itemId = target.getAttribute('data-id');
            
            return {
                itemId: itemId
            };
        }
    });
}
