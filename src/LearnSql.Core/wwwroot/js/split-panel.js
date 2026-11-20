const maxRestoreFrames = 120;
const maxRestoreShare = 0.6;

export function observe(wrapperId, restoreWidth, reference) {
    const wrapper = document.getElementById(wrapperId);
    const container = wrapper ? wrapper.querySelector('.mud-split-panel') : null;
    if (!container || container.children.length !== 3) {
        return { dispose() {} };
    }

    const firstPanel = container.children[0];
    const divider = container.children[1];
    let frame = 0;

    const isStacked = () => getComputedStyle(container).flexDirection === 'column';

    const report = () => {
        const width = Math.round(firstPanel.offsetWidth);
        if (width > 0 && !isStacked()) {
            reference.invokeMethodAsync('OnWidthChanged', width).catch(() => {});
        }
    };
    const reportSoon = () => setTimeout(report, 0);
    const onPointerDown = () => {
        document.addEventListener('mouseup', reportSoon, { once: true });
        document.addEventListener('touchend', reportSoon, { once: true });
    };

    const restore = (attempt) => {
        if (window.splitPanels && window.splitPanels[container.id]) {
            if (!isStacked() && container.offsetWidth > 0) {
                const limit = Math.floor(container.offsetWidth * maxRestoreShare);
                window.mudSplitPanel_setDividerPosition(container.id, Math.min(restoreWidth, limit));
            }
        } else if (attempt < maxRestoreFrames) {
            frame = requestAnimationFrame(() => restore(attempt + 1));
        }
    };

    divider.addEventListener('mousedown', onPointerDown);
    divider.addEventListener('touchstart', onPointerDown, { passive: true });
    divider.addEventListener('dblclick', reportSoon);
    divider.addEventListener('keydown', reportSoon);

    if (restoreWidth) {
        restore(0);
    }

    return {
        dispose() {
            cancelAnimationFrame(frame);
            divider.removeEventListener('mousedown', onPointerDown);
            divider.removeEventListener('touchstart', onPointerDown);
            divider.removeEventListener('dblclick', reportSoon);
            divider.removeEventListener('keydown', reportSoon);
            document.removeEventListener('mouseup', reportSoon);
            document.removeEventListener('touchend', reportSoon);
        },
    };
}
