using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Services.Interop;

public interface IDocumentService
{
    Task SetLanguageAsync(string languageCode, CancellationToken cancellationToken = default);

    Task<bool> PrefersDarkAsync(CancellationToken cancellationToken = default);

    Task ScrollToTopAsync(ElementReference element, CancellationToken cancellationToken = default);
}
