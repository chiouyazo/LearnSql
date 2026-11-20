using System.Reflection;
using System.Text;

namespace LearnSql.Tests.Support;

public sealed class FakeResourceAssembly : Assembly
{
    private readonly Dictionary<string, Func<Stream?>> _resources = new Dictionary<
        string,
        Func<Stream?>
    >(StringComparer.Ordinal);

    public FakeResourceAssembly Add(string name, string text)
    {
        _resources[name] = () => new MemoryStream(Encoding.UTF8.GetBytes(text));
        return this;
    }

    public FakeResourceAssembly AddStream(string name, Func<Stream?> factory)
    {
        _resources[name] = factory;
        return this;
    }

    public override string[] GetManifestResourceNames()
    {
        return [.. _resources.Keys];
    }

    public override Stream? GetManifestResourceStream(string name)
    {
        return _resources.TryGetValue(name, out Func<Stream?>? factory) ? factory() : null;
    }
}
