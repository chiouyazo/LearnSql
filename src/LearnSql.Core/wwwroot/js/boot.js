function readStored(key) {
    try {
        const raw = window.localStorage.getItem(key);
        const value = raw ? JSON.parse(raw) : null;
        return typeof value === 'string' ? value : null;
    } catch {
        return null;
    }
}

const culture = readStored('learnsql:v1:language') || 'de';
const theme = readStored('learnsql:v1:theme');

document.documentElement.lang = culture.split('-')[0];

if (theme) {
    document.documentElement.dataset.theme = theme;
}

Blazor.start({
    configureRuntime: (builder) => {
        builder.withApplicationCulture(culture);
    },
});
