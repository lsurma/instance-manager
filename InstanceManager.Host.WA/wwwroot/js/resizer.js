import interactjs from 'https://cdn.jsdelivr.net/npm/interactjs@1.10.27/+esm';

window.interactJs = {
    makeResizable: function (dotNetHelper, targetElement, propertyElement) {
        const computedStyle = getComputedStyle(targetElement);
        const minWidthVar = computedStyle.getPropertyValue('--dialog-min-width');
        const minWidth = parseInt(minWidthVar, 10) || 100;
        let debounceTimeout;

        interactjs(targetElement)
            .resizable({
                edges: { left: false, right: true, bottom: false, top: false },

                listeners: {
                    move: function (event) {
                        const width = event.rect.width;

                        if (propertyElement) {
                            propertyElement.style.setProperty('--dialog-width', `${width}px`);
                        } else {
                            targetElement.style.width = `${width}px`;
                        }

                        clearTimeout(debounceTimeout);
                        debounceTimeout = setTimeout(() => {
                            dotNetHelper.invokeMethodAsync('SetWidth', width);
                        }, 250);
                    }
                },
                modifiers: [
                    interactjs.modifiers.restrictSize({
                        min: { width: minWidth }
                    })
                ],
                inertia: true
            });
    }
};
