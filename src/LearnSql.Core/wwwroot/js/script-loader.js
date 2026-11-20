const pending = new Map();

export function loadScript(relativeUrl) {
    const url = new URL(relativeUrl, document.baseURI).href;
    const existing = pending.get(url);
    if (existing) {
        return existing;
    }

    const promise = new Promise((resolve, reject) => {
        const element = document.createElement('script');
        element.src = url;
        element.async = false;
        element.onload = () => resolve();
        element.onerror = () => {
            pending.delete(url);
            element.remove();
            reject(new Error('Script could not be loaded: ' + relativeUrl));
        };
        document.head.appendChild(element);
    });
    pending.set(url, promise);
    return promise;
}

export function loadAmdModule(moduleId) {
    return new Promise((resolve, reject) => {
        window.require([moduleId], () => resolve(), (error) => reject(new Error('Module could not be loaded: ' + moduleId + ' ' + error)));
    });
}

export async function configureAmdLanguage(language) {
    const moduleId = 'vs/nls.messages.' + language;
    await loadScript('_content/BlazorMonaco/lib/monaco-editor/min/' + moduleId + '.js.js');
    window.define(moduleId, [moduleId + '.js'], () => ({}));
    window.require.config({ 'vs/nls': { availableLanguages: { '*': language } } });
}
