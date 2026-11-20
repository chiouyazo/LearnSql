using System.Globalization;
using System.Resources;

namespace LearnSql.Sql.Resources;

internal sealed class StringTable
{
    private readonly ResourceManager _manager;

    public StringTable(Type owner)
    {
        _manager = new ResourceManager(owner);
    }

    public string Get(string key)
    {
        return Find(key)
            ?? throw new InvalidOperationException($"The resource '{key}' does not exist.");
    }

    public string? Find(string key)
    {
        return _manager.GetString(key, CultureInfo.CurrentUICulture);
    }

    public string Format(string key, params object?[] arguments)
    {
        return string.Format(CultureInfo.CurrentUICulture, Get(key), arguments);
    }
}
