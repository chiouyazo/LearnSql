using LearnSql.Sql.Dialect.Unsupported;
using LearnSql.Sql.Resources;

namespace LearnSql.Sql.Dialect.Hints;

public static class DialectDifferences
{
    public static IReadOnlyList<DialectHintTopic> CreateDefault(
        IReadOnlyList<UnsupportedConstruct> unsupportedConstructs
    )
    {
        return
        [
            DataTypes(),
            Numbers(),
            DatesAndTimes(),
            Collation(),
            NullBehavior(),
            RowLimiting(),
            Strings(),
            VariablesAndBatches(),
            SchemaObjects(),
            NotYetAvailable(unsupportedConstructs),
        ];
    }

    private static DialectHintTopic DataTypes()
    {
        return new DialectHintTopic(
            "DataTypes",
            [
                new DialectHintExample(
                    "INSERT INTO t (IntSpalte) VALUES ('abc')",
                    () => HintStrings.ExampleMsSqlResult("DataTypes", 1),
                    () => HintStrings.ExampleSandboxResult("DataTypes", 1)
                ),
                new DialectHintExample(
                    "SELECT 2147483647 + 1",
                    () => HintStrings.ExampleMsSqlResult("DataTypes", 2),
                    () => "2147483648"
                ),
            ]
        );
    }

    private static DialectHintTopic Numbers()
    {
        return new DialectHintTopic(
            "Numbers",
            [
                new DialectHintExample("SELECT 7 / 2.0", "3.500000", "3.5"),
                new DialectHintExample(
                    "SELECT AVG(x) FROM (SELECT 1 AS x UNION ALL SELECT 2) t",
                    "1",
                    "1.5"
                ),
                new DialectHintExample("SELECT 0.1 + 0.2", "0.3", "0.30000000000000004"),
            ]
        );
    }

    private static DialectHintTopic DatesAndTimes()
    {
        return new DialectHintTopic(
            "DatesAndTimes",
            [
                new DialectHintExample(
                    "SELECT DATEDIFF(year, '2023-12-31', '2024-01-01')",
                    "1",
                    "1"
                ),
                new DialectHintExample(
                    "SELECT DATEADD(month, 1, '2024-01-31')",
                    "2024-02-29 00:00:00.000",
                    "2024-02-29"
                ),
                new DialectHintExample(
                    "SELECT DATENAME(weekday, '2024-03-05')",
                    "Tuesday",
                    "Tuesday"
                ),
            ]
        );
    }

    private static DialectHintTopic Collation()
    {
        return new DialectHintTopic(
            "Collation",
            [
                new DialectHintExample("SELECT CASE WHEN 'a' = 'A' THEN 1 ELSE 0 END", "1", "0"),
                new DialectHintExample("SELECT CASE WHEN 'a' = 'a ' THEN 1 ELSE 0 END", "1", "0"),
                new DialectHintExample("SELECT CASE WHEN 'Ä' LIKE 'ä' THEN 1 ELSE 0 END", "1", "0"),
            ]
        );
    }

    private static DialectHintTopic NullBehavior()
    {
        return new DialectHintTopic(
            "NullBehavior",
            [
                new DialectHintExample(
                    "SELECT x FROM (SELECT NULL AS x UNION ALL SELECT 1) t ORDER BY x",
                    "NULL, 1",
                    "NULL, 1"
                ),
            ]
        );
    }

    private static DialectHintTopic RowLimiting()
    {
        return new DialectHintTopic(
            "RowLimiting",
            [
                new DialectHintExample(
                    "SELECT TOP 2 Name FROM Customers ORDER BY Id",
                    "Anna, Ben",
                    "Anna, Ben"
                ),
                new DialectHintExample("SELECT 1 UNION SELECT 2 INTERSECT SELECT 3", "1", "1"),
            ]
        );
    }

    private static DialectHintTopic Strings()
    {
        return new DialectHintTopic(
            "Strings",
            [
                new DialectHintExample("SELECT 'Nr. ' + CAST(5 AS VARCHAR(10))", "Nr. 5", "Nr. 5"),
                new DialectHintExample("SELECT '2' + '3'", "23", "23"),
                new DialectHintExample("SELECT '1' + 1", "2", "2"),
            ]
        );
    }

    private static DialectHintTopic VariablesAndBatches()
    {
        return new DialectHintTopic(
            "VariablesAndBatches",
            [
                new DialectHintExample("DECLARE @t VARCHAR(3) = 'abcdef'; SELECT @t", "abc", "abc"),
                new DialectHintExample("DECLARE @i INT = 1; SET @i += 4; SELECT @i", "5", "5"),
            ]
        );
    }

    private static DialectHintTopic SchemaObjects()
    {
        return new DialectHintTopic(
            "SchemaObjects",
            [
                new DialectHintExample(
                    "CREATE TABLE P (Id INT IDENTITY(10, 5) PRIMARY KEY); INSERT INTO P DEFAULT VALUES; SELECT Id FROM P",
                    "10",
                    "1"
                ),
            ]
        );
    }

    private static DialectHintTopic NotYetAvailable(
        IReadOnlyList<UnsupportedConstruct> unsupportedConstructs
    )
    {
        return new DialectHintTopic(
            "NotYetAvailable",
            [],
            () =>
                unsupportedConstructs
                    .Select(
                        (UnsupportedConstruct construct) =>
                            $"{construct.Name}. {construct.Alternative}"
                    )
                    .ToList()
        );
    }
}
