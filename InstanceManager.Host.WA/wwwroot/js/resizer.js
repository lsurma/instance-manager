window.interactJs = {
    makeResizable: function (dotNetHelper, element) {
        interact(element)
            .resizable({
                edges: { left: false, right: true, bottom: false, top: false },

                listeners: {
                    move: function (event) {
                        let target = event.target;

                        target.style.width = event.rect.width + 'px';

                        dotNetHelper.invokeMethodAsync('SetWidth', event.rect.width);
                    }
                },
                modifiers: [
                    interact.modifiers.restrictSize({
                        min: { width: 100 }
                    })
                ],
                inertia: true
            });
    }
};
