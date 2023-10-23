import DOMCleanup from '/js/DOMCleanup.js';

export const coListeners = {};

export function registerClickOutsideHelper(elementId, dotnetHelper) {
    coListeners[elementId] = (e) => {
        if (!document.getElementById(elementId).contains(e.target)) {
            dotnetHelper.invokeMethodAsync("ClickedOutside");
        }
    }

    window.addEventListener("click", coListeners[elementId], true);

    DOMCleanup.listen(elementId, () => {
        window.removeEventListener("click", coListeners[elementId], true);
        delete coListeners[elementId];
    });
};