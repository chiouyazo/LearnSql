namespace LearnSql.Sql.Resources;

internal static class UnsupportedStrings
{
    private const string NameSuffix = "_Name";
    private const string AlternativeSuffix = "_Alternative";

    private static readonly StringTable Table = new StringTable(typeof(UnsupportedStrings));

    public static string Name(string constructId)
    {
        return Table.Get(constructId + NameSuffix);
    }

    public static string Alternative(string constructId, params object?[] arguments)
    {
        return Table.Format(constructId + AlternativeSuffix, arguments);
    }
}
