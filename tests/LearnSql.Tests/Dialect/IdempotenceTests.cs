using LearnSql.Sql.Dialect;

namespace LearnSql.Tests.Dialect;

public class IdempotenceTests
{
    private readonly TSqlDialectTranslator _translator = TSqlDialectTranslator.CreateDefault();

    [Theory]
    [InlineData("SELECT 1")]
    [InlineData("select a, b from t where a = 1 and b <> 'x' order by b desc")]
    [InlineData("SELECT t1.a, t2.b FROM T t1 INNER JOIN T2 t2 ON t1.a = t2.a")]
    [InlineData("SELECT COUNT(*) AS n FROM T GROUP BY a HAVING COUNT(*) > 1")]
    [InlineData("SELECT CASE WHEN a > 1 THEN 'big' ELSE 'small' END FROM T")]
    [InlineData("SELECT a FROM T WHERE a IN (1, 2, 3) AND b LIKE 'x%'")]
    [InlineData("SELECT a FROM T WHERE EXISTS (SELECT 1 FROM S WHERE S.a = T.a)")]
    [InlineData("SELECT a, ROW_NUMBER() OVER (PARTITION BY b ORDER BY a) AS rn FROM T")]
    [InlineData("WITH c AS (SELECT 1 AS n) SELECT n FROM c")]
    [InlineData("SELECT a FROM T UNION ALL SELECT a FROM S")]
    [InlineData("INSERT INTO T (a, b) VALUES (1, 'x')")]
    [InlineData("UPDATE T SET a = 2 WHERE b = 3")]
    [InlineData("DELETE FROM T WHERE a = 1")]
    [InlineData("CREATE TABLE T (a INT, b VARCHAR(5))")]
    [InlineData("DROP TABLE T")]
    [InlineData("SELECT /* inner */ 1")]
    [InlineData("SELECT\n    a,\n    b\nFROM T")]
    public void Translate_PlainSql_RemainsCharacterIdentical(string sql)
    {
        DialectTranslationResult result = _translator.Translate(sql);

        Assert.True(result.Success, string.Join("; ", result.Errors.Select(e => e.Message)));
        TranslatedStatement statement = Assert.Single(Assert.Single(result.Batches).Statements);
        Assert.Equal(sql, statement.Sql);
    }

    [Fact]
    public void Translate_TrailingSemicolon_IsRemovedAndNothingElseChanges()
    {
        DialectTranslationResult result = _translator.Translate("SELECT a FROM T;");

        Assert.Equal(
            "SELECT a FROM T",
            Assert.Single(Assert.Single(result.Batches).Statements).Sql
        );
    }

    [Fact]
    public void Translate_SameInputTwice_YieldsTheSameOutput()
    {
        const string sql = "SELECT TOP 3 [a] + 'x' FROM [dbo].[T] ORDER BY 1";

        DialectTranslationResult first = _translator.Translate(sql);
        DialectTranslationResult second = _translator.Translate(sql);

        Assert.Equal(
            first.Batches.SelectMany(
                (TranslatedBatch b) => b.Statements.Select((TranslatedStatement s) => s.Sql)
            ),
            second.Batches.SelectMany(
                (TranslatedBatch b) => b.Statements.Select((TranslatedStatement s) => s.Sql)
            )
        );
    }
}
