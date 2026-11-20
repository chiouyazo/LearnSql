using System.Globalization;
using System.Runtime.CompilerServices;

namespace LearnSql.Tests.Support;

internal static class NeutralTestCulture
{
    [ModuleInitializer]
    internal static void Apply()
    {
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
    }
}
