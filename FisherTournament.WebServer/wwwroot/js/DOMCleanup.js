class DOMCleanup {
    constructor() {
        this.listeners = new Map();
        this.setupMutationObserver();
    }

    listen(id, callback) {
        if (!this.listeners.has(id)) {
            this.listeners.set(id, []);
        }
        this.listeners.get(id).push(callback);
    }

    setupMutationObserver() {
        const observer = new MutationObserver(mutations => {
            mutations.forEach(mutation => {
                if (mutation.removedNodes.length > 0) {
                    const removedIds = new Set();
                    mutation.removedNodes.forEach(node => {
                        if (node.id) {
                            removedIds.add(node.id);
                        }
                    });

                    removedIds.forEach(id => {
                        if (this.listeners.has(id)) {
                            this.listeners.get(id).forEach(callback => callback());
                        }
                    });
                }
            });
        });

        observer.observe(document.body, { childList: true, subtree: true });
    }
}

const cleanup = new DOMCleanup();
export default cleanup;