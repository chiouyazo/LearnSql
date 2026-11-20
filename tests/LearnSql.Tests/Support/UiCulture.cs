using System.Globalization;

namespace LearnSql.Tests.Support;

public sealed class UiCulture : IDisposable
{
    private readonly CultureInfo _previous;

    private UiCulture(CultureInfo previous)
    {
        _previous = previous;
    }

    public static UiCulture Use(string name)
    {
        UiCulture scope = new UiCulture(CultureInfo.CurrentUICulture);
        CultureInfo.CurrentUICulture = new CultureInfo(name);
        return scope;
    }

    public void Dispose()
    {
        CultureInfo.CurrentUICulture = _previous;
    }
}
