namespace LearnSql.Sql.Resources;

internal static class DialectStrings
{
    private static readonly StringTable Table = new StringTable(typeof(DialectStrings));

    public static string IgnoredTableHint => Table.Get(nameof(IgnoredTableHint));

    public static string InternalTranslationError => Table.Get(nameof(InternalTranslationError));

    public static string ConstructNotAvailable(string name)
    {
        return Table.Format(nameof(ConstructNotAvailable), name);
    }

    public static string IgnoredStatement(string statement)
    {
        return Table.Format(nameof(IgnoredStatement), statement);
    }

    public static string MeantKeyword(string keyword)
    {
        return Table.Format(nameof(MeantKeyword), keyword);
    }

    public static string MeantKeywordForImplicitExecute(string keyword)
    {
        return Table.Format(nameof(MeantKeywordForImplicitExecute), keyword);
    }
}
