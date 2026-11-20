export function setLanguage(languageCode) {
    document.documentElement.lang = languageCode;
}

export function prefersDark() {
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
}

export function scrollToTop(element) {
    if (element) {
        element.scrollIntoView({ block: 'start', behavior: 'smooth' });
    }
}
