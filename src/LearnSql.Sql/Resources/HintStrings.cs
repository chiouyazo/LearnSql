namespace LearnSql.Sql.Resources;

internal static class HintStrings
{
    private const string TitleSuffix = "_Title";
    private const string SummarySuffix = "_Summary";
    private const string PointInfix = "_Point";
    private const string ExampleInfix = "_Example";
    private const string MsSqlSuffix = "_MsSql";
    private const string SandboxSuffix = "_Sandbox";

    private static readonly StringTable Table = new StringTable(typeof(HintStrings));

    public static string Title(string topicId)
    {
        return Table.Get(topicId + TitleSuffix);
    }

    public static string Summary(string topicId)
    {
        return Table.Get(topicId + SummarySuffix);
    }

    public static IReadOnlyList<string> Points(string topicId)
    {
        List<string> points = [];
        string? point = Table.Find(PointKey(topicId, 1));
        while (point is not null)
        {
            points.Add(point);
            point = Table.Find(PointKey(topicId, points.Count + 1));
        }

        return points;
    }

    public static string ExampleMsSqlResult(string topicId, int number)
    {
        return Table.Get(ExampleKey(topicId, number, MsSqlSuffix));
    }

    public static string ExampleSandboxResult(string topicId, int number)
    {
        return Table.Get(ExampleKey(topicId, number, SandboxSuffix));
    }

    private static string PointKey(string topicId, int number)
    {
        return topicId + PointInfix + number;
    }

    private static string ExampleKey(string topicId, int number, string suffix)
    {
        return topicId + ExampleInfix + number + suffix;
    }
}
