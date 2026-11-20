using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Dialect;

public class TopAndSetOperatorTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public TopAndSetOperatorTests()
    {
        _database.ExecuteRaw(
            "CREATE TABLE N (v INT NOT NULL); INSERT INTO N VALUES (1), (2), (3), (4), (5);"
        );
    }

    public void Dispose()
    {
        _database.Dispose();
    }

    private IReadOnlyList<object?> Values(string sql) => _database.Query(sql).Column();

    [Fact]
    public void Top_WithOrderBy_ReturnsTheFirstRowsOfTheSortedResult()
    {
        Assert.Equal([1L, 2L], Values("SELECT TOP 2 v FROM N ORDER BY v"));
    }

    [Fact]
    public void Top_WithDescendingOrder_ReturnsTheLargestRows()
    {
        Assert.Equal([5L, 4L, 3L], Values("SELECT TOP 3 v FROM N ORDER BY v DESC"));
    }

    [Fact]
    public void Top_WithParenthesizedCount_IsAccepted()
    {
        Assert.Equal([1L, 2L], Values("SELECT TOP (2) v FROM N ORDER BY v"));
    }

    [Fact]
    public void Top_Zero_ReturnsNoRows()
    {
        Assert.Empty(Values("SELECT TOP 0 v FROM N"));
    }

    [Fact]
    public void Top_LargerThanTheTable_ReturnsAllRows()
    {
        Assert.Equal(5, Values("SELECT TOP 100 v FROM N").Count);
    }

    [Fact]
    public void Top_WithVariable_UsesTheVariableValue()
    {
        Assert.Equal([1L, 2L], Values("DECLARE @n INT = 2; SELECT TOP (@n) v FROM N ORDER BY v"));
    }

    [Fact]
    public void Top_WithVariableChangedBySet_UsesTheCurrentValue()
    {
        Assert.Equal(
            [1L, 2L, 3L],
            Values("DECLARE @n INT = 1; SET @n = @n + 2; SELECT TOP (@n) v FROM N ORDER BY v")
        );
    }

    [Fact]
    public void Top_InDerivedTable_LimitsBeforeTheOuterQuery()
    {
        Assert.Equal(
            [4L, 5L],
            Values("SELECT v FROM (SELECT TOP 2 v FROM N ORDER BY v DESC) d ORDER BY v")
        );
    }

    [Fact]
    public void Top_InSubqueryOfWhere_LimitsTheSubquery()
    {
        Assert.Equal(
            [1L, 2L],
            Values("SELECT v FROM N WHERE v IN (SELECT TOP 2 v FROM N ORDER BY v) ORDER BY v")
        );
    }

    [Fact]
    public void Top_InCommonTableExpression_LimitsTheExpression()
    {
        Assert.Equal(
            [1L, 2L],
            Values("WITH c AS (SELECT TOP 2 v FROM N ORDER BY v) SELECT v FROM c ORDER BY v")
        );
    }

    [Fact]
    public void Top_InInsertSelect_LimitsTheInsertedRows()
    {
        _database.RunSuccessfully("CREATE TABLE M (v INT)");

        _database.RunSuccessfully("INSERT INTO M SELECT TOP 2 v FROM N ORDER BY v DESC");

        Assert.Equal(2L, _database.Scalar("SELECT COUNT(*) FROM M"));
    }

    [Fact]
    public void Top_OnEveryMemberOfUnionAll_LimitsEachMemberSeparately()
    {
        Assert.Equal([1L, 1L, 2L], Values("SELECT TOP 1 v FROM N UNION ALL SELECT TOP 2 v FROM N"));
    }

    [Fact]
    public void Top_OnParenthesizedUnionMembers_LimitsEachMemberSeparately()
    {
        Assert.Equal([1L, 2L], Values("(SELECT TOP 1 1 AS v) UNION ALL (SELECT TOP 2 2)"));
    }

    [Fact]
    public void Top_OnlyOnTheSecondUnionMember_LeavesTheFirstMemberUnlimited()
    {
        Assert.Equal(
            [1L, 2L, 3L, 4L, 5L, 1L],
            Values("SELECT v FROM N UNION ALL SELECT TOP 1 v FROM N")
        );
    }

    [Fact]
    public void Top_OnUnionMembers_DoesNotLimitTheWholeUnion()
    {
        Assert.Equal(
            [1L, 2L, 3L],
            Values("SELECT TOP 1 v FROM N WHERE v = 1 UNION ALL SELECT TOP 2 v FROM N WHERE v > 1")
        );
    }

    [Fact]
    public void Top_TextInsideStringLiteral_IsNotRewritten()
    {
        Assert.Equal("SELECT TOP 3 x", _database.Scalar("SELECT 'SELECT TOP 3 x' AS s"));
    }

    [Fact]
    public void Top_TextInsideCommentsAndStrings_DoesNotDisturbTheRealTop()
    {
        ScriptOutcome outcome = _database.RunSuccessfully(
            "SELECT TOP 1 'TOP 5' AS x /* TOP 7 */ -- TOP 9"
        );

        Assert.Equal("TOP 5", outcome.LastResultSet.Single());
    }

    [Fact]
    public void Top_TextInsideBracketIdentifier_IsNotRewritten()
    {
        Assert.Equal(1L, _database.Scalar("SELECT TOP 1 [v] AS [TOP 2] FROM N ORDER BY v"));
    }

    [Fact]
    public void Top_WithPercent_IsRejectedAsUnsupported()
    {
        DialectTranslationResult result = _database.Translate("SELECT TOP 10 PERCENT v FROM N");

        Assert.Equal("TopPercentOrTies", Assert.Single(result.Diagnostics).Code);
    }

    [Fact]
    public void Top_WithTies_IsRejectedAsUnsupported()
    {
        DialectTranslationResult result = _database.Translate(
            "SELECT TOP 1 WITH TIES v FROM N ORDER BY v"
        );

        Assert.Equal("TopPercentOrTies", Assert.Single(result.Diagnostics).Code);
    }

    [Fact]
    public void OffsetFetch_FirstRowsAfterSkipping_AreReturned()
    {
        Assert.Equal(
            [2L, 3L],
            Values("SELECT v FROM N ORDER BY v OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY")
        );
    }

    [Fact]
    public void OffsetWithoutFetch_ReturnsEverythingAfterTheSkippedRows()
    {
        Assert.Equal([4L, 5L], Values("SELECT v FROM N ORDER BY v OFFSET 3 ROWS"));
    }

    [Fact]
    public void OffsetFetch_WithVariables_UsesTheirValues()
    {
        Assert.Equal(
            [3L, 4L],
            Values(
                "DECLARE @skip INT = 2; DECLARE @take INT = 2; "
                    + "SELECT v FROM N ORDER BY v OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY"
            )
        );
    }

    [Fact]
    public void TopCombinedWithOffset_IsReportedAsMsSqlError10741()
    {
        DialectTranslationResult result = _database.Translate(
            "SELECT TOP 2 v FROM N ORDER BY v OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY"
        );

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.False(result.Success);
        Assert.Equal(10741, diagnostic.MsSqlErrorNumber);
        Assert.Equal("TopWithOffset", diagnostic.Code);
        Assert.Equal(DiagnosticKind.TranslationError, diagnostic.Kind);
        Assert.Equal(8, diagnostic.Span.Start.Column);
    }

    [Fact]
    public void TopCombinedWithOffset_InASubquery_IsReportedToo()
    {
        DialectTranslationResult result = _database.Translate(
            "SELECT * FROM (SELECT TOP 2 v FROM N ORDER BY v OFFSET 1 ROWS) d"
        );

        Assert.Equal(10741, Assert.Single(result.Diagnostics).MsSqlErrorNumber);
    }

    [Fact]
    public void Intersect_BindsTighterThanUnion_WhenUnionComesFirst()
    {
        Assert.Equal([1L], Values("SELECT 1 UNION SELECT 2 INTERSECT SELECT 3"));
    }

    [Fact]
    public void Intersect_BindsTighterThanUnionAll_WhenUnionAllComesFirst()
    {
        Assert.Equal(
            [1L, 2L, 5L],
            Values("SELECT 1 UNION ALL SELECT 2 INTERSECT SELECT 2 UNION ALL SELECT 5")
                .Order()
                .ToList()
        );
    }

    [Fact]
    public void Intersect_BindsTighterThanUnion_InAChainOfThree()
    {
        Assert.Equal(
            [1L, 4L],
            Values("SELECT 1 UNION SELECT 2 INTERSECT SELECT 3 UNION SELECT 4").Order().ToList()
        );
    }

    [Fact]
    public void Intersect_BindsTighterThanExcept_WhenExceptComesFirst()
    {
        Assert.Equal([1L], Values("SELECT 1 EXCEPT SELECT 2 INTERSECT SELECT 2"));
    }

    [Fact]
    public void Intersect_AfterUnion_WithSeveralIntersectOperands_GroupsAllIntersects()
    {
        Assert.Equal(
            [1L, 2L],
            Values("SELECT 1 UNION SELECT 2 INTERSECT SELECT 2 INTERSECT SELECT 2").Order().ToList()
        );
    }

    [Fact]
    public void Intersect_AtTheStart_KeepsLeftToRightMeaning()
    {
        Assert.Equal([1L, 4L], Values("SELECT 1 INTERSECT SELECT 1 UNION SELECT 4"));
    }

    [Fact]
    public void ExceptFollowedByUnion_IsEvaluatedLeftToRight()
    {
        Assert.Equal([7L], Values("SELECT 5 EXCEPT SELECT 5 UNION SELECT 7"));
    }

    [Fact]
    public void UnionAllWithoutIntersect_IsNotChanged()
    {
        ScriptOutcome outcome = _database.RunSuccessfully(
            "SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3"
        );

        Assert.Equal(
            "SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3",
            outcome.Translation.Batches[0].Statements[0].Sql
        );
    }
}
