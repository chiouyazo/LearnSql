using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Rewriting;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Dialect;

public class VariableTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public void Dispose()
    {
        _database.Dispose();
    }

    [Fact]
    public void Declare_WithInitialValue_IsReadable()
    {
        Assert.Equal(5L, _database.Scalar("DECLARE @i INT = 5; SELECT @i"));
    }

    [Fact]
    public void Declare_WithoutInitialValue_IsNull()
    {
        Assert.Null(_database.Scalar("DECLARE @i INT; SELECT @i"));
    }

    [Fact]
    public void Declare_SeveralVariablesInOneStatement_AreAllDeclared()
    {
        ResultSet result = _database.Query("DECLARE @a INT, @b INT = 3; SELECT @a, @b");

        Assert.Null(result.Rows[0][0]);
        Assert.Equal(3L, result.Rows[0][1]);
    }

    [Fact]
    public void Declare_InitialValueReferencingAnEarlierStatementVariable_UsesIt()
    {
        Assert.Equal(
            2L,
            _database.Scalar("DECLARE @a INT = 1; DECLARE @b INT = @a + 1; SELECT @b")
        );
    }

    [Fact]
    public void VariableNames_AreCaseInsensitive()
    {
        Assert.Equal(1L, _database.Scalar("DECLARE @Total INT = 1; SELECT @TOTAL"));
    }

    [Fact]
    public void Declare_SameNameDifferingOnlyInCase_IsAlreadyDeclared()
    {
        DialectTranslationResult result = _database.Translate(
            "DECLARE @x INT = 1; DECLARE @X INT = 2"
        );

        Assert.Equal(134, Assert.Single(result.Diagnostics).MsSqlErrorNumber);
    }

    [Fact]
    public void Declare_VariableTwiceInTheSameBatch_ReportsMsSqlError134()
    {
        DialectTranslationResult result = _database.Translate("DECLARE @i INT; DECLARE @i INT");

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.False(result.Success);
        Assert.Equal(134, diagnostic.MsSqlErrorNumber);
        Assert.Equal("VariableAlreadyDeclared", diagnostic.Code);
        Assert.Equal(DiagnosticKind.TranslationError, diagnostic.Kind);
        Assert.Contains("'@i'", diagnostic.Message);
        Assert.Equal(1, diagnostic.Span.Start.Line);
        Assert.Equal(25, diagnostic.Span.Start.Column);
    }

    [Fact]
    public void Declare_SameVariableInDifferentBatches_IsAllowed()
    {
        ScriptOutcome outcome = _database.RunSuccessfully(
            "DECLARE @i INT = 1; SELECT @i\nGO\nDECLARE @i INT = 2; SELECT @i"
        );

        Assert.Equal([1L, 2L], outcome.ResultSets.Select((ResultSet r) => r.Single()));
    }

    [Fact]
    public void Reference_InTheBatchAfterGo_ReportsMsSqlError137()
    {
        DialectTranslationResult result = _database.Translate("DECLARE @x INT = 1\nGO\nSELECT @x");

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(137, diagnostic.MsSqlErrorNumber);
        Assert.Equal("UndeclaredVariable", diagnostic.Code);
        Assert.Contains("@x", diagnostic.Message);
        Assert.Equal(3, diagnostic.Span.Start.Line);
        Assert.Equal(8, diagnostic.Span.Start.Column);
    }

    [Theory]
    [InlineData("SELECT @x")]
    [InlineData("SET @x = 1")]
    [InlineData("DECLARE @a INT = @x")]
    [InlineData("SELECT @x; DECLARE @x INT")]
    [InlineData("SELECT 1 WHERE 1 = @x")]
    public void Reference_OfAnUndeclaredVariable_ReportsMsSqlError137(string sql)
    {
        DialectTranslationResult result = _database.Translate(sql);

        Assert.False(result.Success);
        Assert.Equal(137, Assert.Single(result.Diagnostics).MsSqlErrorNumber);
    }

    [Theory]
    [InlineData("DECLARE @a INT = 1, @b INT = @a + 1", "@a")]
    [InlineData("DECLARE @a INT = @b, @b INT = 1", "@b")]
    [InlineData("DECLARE @a INT = @a", "@a")]
    [InlineData("DECLARE @a INT = 1, @b INT = (SELECT @a)", "@a")]
    [InlineData("DECLARE @a INT = 1, @b INT = CAST(@a AS INT)", "@a")]
    public void Declare_InitialValueReferencingAVariableOfTheSameStatement_ReportsMsSqlError137(
        string sql,
        string variable
    )
    {
        DialectTranslationResult result = _database.Translate(sql);

        Assert.False(result.Success);
        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("UndeclaredVariable", diagnostic.Code);
        Assert.Equal(137, diagnostic.MsSqlErrorNumber);
        Assert.Equal($"Must declare the scalar variable \"{variable}\".", diagnostic.Message);
    }

    [Fact]
    public void Declare_InitialValueUsingTwoVariablesOfTheSameStatement_ReportsBoth()
    {
        DialectTranslationResult result = _database.Translate(
            "DECLARE @a INT = 1, @b INT = 2, @c INT = @a + @b"
        );

        Assert.Equal(
            [
                "Must declare the scalar variable \"@a\".",
                "Must declare the scalar variable \"@b\".",
            ],
            result.Diagnostics.Select((DialectDiagnostic diagnostic) => diagnostic.Message)
        );
    }

    [Fact]
    public void Declare_SeveralVariablesWithInitialValues_AreAllSet()
    {
        ResultSet result = _database.Query(
            "DECLARE @a INT = 1, @b INT = 2, @c VARCHAR(5) = 'x', @d INT; SELECT @a, @b, @c, @d"
        );

        Assert.Equal([1L, 2L, "x", null], result.Rows.Single());
    }

    [Fact]
    public void Declare_LaterStatementReferencingEarlierMultiDeclaration_UsesTheValues()
    {
        Assert.Equal(
            3L,
            _database.Scalar("DECLARE @a INT = 1, @b INT = 2; DECLARE @c INT = @a + @b; SELECT @c")
        );
    }

    [Fact]
    public void Declare_ChainOfSeparateStatements_EvaluatesInOrder()
    {
        ResultSet result = _database.Query(
            "DECLARE @a INT = 1; DECLARE @b INT = @a + 1; DECLARE @c INT = @b * 10, @d INT = @a + @b; SELECT @a, @b, @c, @d"
        );

        Assert.Equal([1L, 2L, 20L, 3L], result.Rows.Single());
    }

    [Fact]
    public void Declare_SeparateStatementsOnOneLine_AllowReferences()
    {
        Assert.Equal(
            2L,
            _database.Scalar("DECLARE @a INT = 1; DECLARE @b INT = @a + 1; SELECT @b")
        );
    }

    [Fact]
    public void Declare_ReferencingAVariableOfTheSameStatementInASubsequentSet_UsesTheValue()
    {
        Assert.Equal(
            2L,
            _database.Scalar("DECLARE @a INT = 1, @b INT; SET @b = @a + 1; SELECT @b")
        );
    }

    [Fact]
    public void Declare_VariablesOfOneStatement_CanBeUsedInTheFollowingSelect()
    {
        ResultSet result = _database.Query(
            "DECLARE @a INT = 4, @b INT = 6; SELECT @a + @b AS Total, @a * @b AS Product"
        );

        Assert.Equal([10L, 24L], result.Rows.Single());
    }

    [Fact]
    public void Variables_AreResetBetweenRepetitionsOfAGoCountBatch()
    {
        ScriptOutcome outcome = _database.RunSuccessfully(
            "DECLARE @i INT = 0; SET @i += 1; SELECT @i\nGO 3"
        );

        Assert.Equal([1L, 1L, 1L], outcome.ResultSets.Select((ResultSet r) => r.Single()));
    }

    [Fact]
    public void Variables_DoNotLeakIntoTheNextRun()
    {
        _database.RunSuccessfully("DECLARE @i INT = 1");

        DialectTranslationResult result = _database.Translate("SELECT @i");

        Assert.Equal(137, Assert.Single(result.Diagnostics).MsSqlErrorNumber);
    }

    [Fact]
    public void Set_PlainAssignment_ReplacesTheValue()
    {
        Assert.Equal(7L, _database.Scalar("DECLARE @i INT = 1; SET @i = 7; SELECT @i"));
    }

    [Fact]
    public void Set_FromAnotherVariableExpression_UsesTheCurrentValues()
    {
        Assert.Equal(
            10L,
            _database.Scalar("DECLARE @a INT = 4; DECLARE @b INT = 6; SET @a = @a + @b; SELECT @a")
        );
    }

    [Fact]
    public void Set_FromSubquery_AssignsTheQueryResult()
    {
        _database.ExecuteRaw("CREATE TABLE N (v INT); INSERT INTO N VALUES (4), (9);");

        Assert.Equal(
            9L,
            _database.Scalar("DECLARE @m INT; SET @m = (SELECT MAX(v) FROM N); SELECT @m")
        );
    }

    [Theory]
    [InlineData("SET @i += 5", 15L)]
    [InlineData("SET @i -= 3", 7L)]
    [InlineData("SET @i *= 2", 20L)]
    [InlineData("SET @i /= 4", 2L)]
    [InlineData("SET @i %= 4", 2L)]
    [InlineData("SET @i &= 6", 2L)]
    [InlineData("SET @i |= 5", 15L)]
    public void Set_CompoundAssignment_AppliesTheOperator(string statement, long expected)
    {
        Assert.Equal(expected, _database.Scalar($"DECLARE @i INT = 10; {statement}; SELECT @i"));
    }

    [Fact]
    public void Set_PlusEqualsOnText_Concatenates()
    {
        Assert.Equal(
            "ab",
            _database.Scalar("DECLARE @s VARCHAR(10) = 'a'; SET @s += 'b'; SELECT @s")
        );
    }

    [Fact]
    public void Set_PlusEqualsOnTextWithNull_YieldsNull()
    {
        Assert.Null(_database.Scalar("DECLARE @s VARCHAR(10) = 'a'; SET @s += NULL; SELECT @s"));
    }

    [Fact]
    public void Set_PlusEqualsOnTextWithTextVariable_Concatenates()
    {
        Assert.Equal(
            "ab",
            _database.Scalar(
                "DECLARE @s VARCHAR(10) = 'a'; DECLARE @t VARCHAR(10) = 'b'; SET @s += @t; SELECT @s"
            )
        );
    }

    [Fact]
    public void Set_PlusEqualsOnNumberWithNumericText_Adds()
    {
        Assert.Equal(6L, _database.Scalar("DECLARE @i INT = 1; SET @i += '5'; SELECT @i"));
    }

    [Fact]
    public void Declare_VarcharWithLength_TruncatesTheInitialValue()
    {
        Assert.Equal("abc", _database.Scalar("DECLARE @t VARCHAR(3) = 'abcdef'; SELECT @t"));
    }

    [Fact]
    public void Set_VarcharWithLength_TruncatesTheAssignedValue()
    {
        Assert.Equal(
            "wxy",
            _database.Scalar("DECLARE @t VARCHAR(3) = 'a'; SET @t = 'wxyz12'; SELECT @t")
        );
    }

    [Fact]
    public void Set_PlusEqualsOnVarcharWithLength_TruncatesTheConcatenation()
    {
        Assert.Equal(
            "abcd",
            _database.Scalar("DECLARE @s VARCHAR(4) = 'ab'; SET @s += 'cdef'; SELECT @s")
        );
    }

    [Fact]
    public void Declare_VarcharMax_DoesNotTruncate()
    {
        string longText = new string('x', 5000);

        object? value = _database.Scalar($"DECLARE @t VARCHAR(MAX) = '{longText}'; SELECT @t");

        Assert.Equal(longText, value);
    }

    [Fact]
    public void Set_IntVariableWithFraction_TruncatesToInteger()
    {
        Assert.Equal(3L, _database.Scalar("DECLARE @i INT = 0; SET @i = 3.7; SELECT @i"));
    }

    [Fact]
    public void Declare_DateVariable_KeepsTheDateText()
    {
        Assert.Equal("2024-03-15", _database.Scalar("DECLARE @d DATE = '2024-03-15'; SELECT @d"));
    }

    [Fact]
    public void Declare_DateVariable_WorksWithDateFunctions()
    {
        Assert.Equal(
            "2024-03-16",
            _database.Scalar("DECLARE @d DATE = '2024-03-15'; SELECT DATEADD(day, 1, @d)")
        );
    }

    [Fact]
    public void Variable_InWhereClause_FiltersRows()
    {
        _database.ExecuteRaw("CREATE TABLE N (v INT); INSERT INTO N VALUES (1), (2), (3);");

        Assert.Equal(
            2L,
            _database.Scalar("DECLARE @m INT = 1; SELECT COUNT(*) FROM N WHERE v > @m")
        );
    }

    [Fact]
    public void Variable_InInsertValues_IsStored()
    {
        _database.ExecuteRaw("CREATE TABLE N (v INT);");

        _database.RunSuccessfully("DECLARE @x INT = 42; INSERT INTO N VALUES (@x)");

        Assert.Equal(42L, _database.Scalar("SELECT v FROM N"));
    }

    [Fact]
    public void Variable_InUpdateAndDelete_IsUsedInThePredicate()
    {
        _database.ExecuteRaw("CREATE TABLE N (v INT); INSERT INTO N VALUES (1), (2), (3);");

        _database.RunSuccessfully("DECLARE @m INT = 1; UPDATE N SET v = v + 10 WHERE v > @m");
        _database.RunSuccessfully("DECLARE @m INT = 12; DELETE FROM N WHERE v = @m");

        Assert.Equal([1L, 13L], _database.Query("SELECT v FROM N ORDER BY v").Column());
    }

    [Fact]
    public void Variable_InCaseExpression_IsEvaluated()
    {
        Assert.Equal(
            "big",
            _database.Scalar(
                "DECLARE @i INT = 5; SELECT CASE WHEN @i > 3 THEN 'big' ELSE 'small' END"
            )
        );
    }

    [Fact]
    public void Variable_PassedToFunctions_IsReadAsItsValue()
    {
        Assert.Equal(3L, _database.Scalar("DECLARE @v VARCHAR(5) = 'abc'; SELECT LEN(@v)"));
    }

    [Fact]
    public void Select_OfBareVariable_UsesTheNoColumnNameHeader()
    {
        ResultSet result = _database.Query("DECLARE @x INT = 1; SELECT @x");

        Assert.Equal(["(No column name)"], result.Columns);
    }

    [Fact]
    public void Select_OfSeveralBareVariables_EachGetsTheNoColumnNameHeader()
    {
        ResultSet result = _database.Query("DECLARE @x INT = 1; DECLARE @y INT = 2; SELECT @x, @y");

        Assert.Equal(["(No column name)", "(No column name)"], result.Columns);
    }

    [Fact]
    public void Select_OfVariableWithAlias_UsesTheAlias()
    {
        ResultSet result = _database.Query("DECLARE @x INT = 1; SELECT @x AS Value");

        Assert.Equal(["Value"], result.Columns);
    }

    [Fact]
    public void Select_OfVariableAmongNamedColumns_OnlyNamesTheBareVariable()
    {
        ResultSet result = _database.Query("DECLARE @x INT = 1; SELECT @x, 2 AS two");

        Assert.Equal(["(No column name)", "two"], result.Columns);
    }

    [Fact]
    public void Statements_WithVariables_StartWithGeneratedSetupStatementsAtTheFirstOrigin()
    {
        DialectTranslationResult result = _database.Translate("DECLARE @x INT = 1; SELECT @x");

        IReadOnlyList<TranslatedStatement> statements = Assert.Single(result.Batches).Statements;
        Assert.Equal(VariableStorage.SetupStatements.Count + 2, statements.Count);
        Assert.All(
            statements.Take(VariableStorage.SetupStatements.Count),
            (TranslatedStatement statement) =>
            {
                Assert.True(statement.IsGenerated);
                Assert.Equal(
                    statements[VariableStorage.SetupStatements.Count].Origin,
                    statement.Origin
                );
            }
        );
        Assert.All(
            statements.Skip(VariableStorage.SetupStatements.Count),
            (TranslatedStatement statement) => Assert.False(statement.IsGenerated)
        );
    }

    [Fact]
    public void Statements_WithoutVariables_HaveNoGeneratedStatements()
    {
        DialectTranslationResult result = _database.Translate("SELECT 1");

        Assert.DoesNotContain(
            Assert.Single(result.Batches).Statements,
            (TranslatedStatement statement) => statement.IsGenerated
        );
    }

    [Fact]
    public void VariableStorage_TableName_IsTheDocumentedInternalName()
    {
        Assert.Equal("__learnsql_variables", VariableStorage.TableName);
    }

    [Fact]
    public void Variables_AreStoredInATemporaryTableAndNeverInTheMainSchema()
    {
        _database.RunSuccessfully("DECLARE @x INT = 1");

        Assert.Equal(
            0L,
            _database.Scalar(
                "SELECT COUNT(*) FROM sqlite_master WHERE name = '__learnsql_variables'"
            )
        );
    }

    [Fact]
    public void Variable_InSelectListOfBatchWithTwoGoSeparatedDeclarations_KeepsBatchesApart()
    {
        ScriptOutcome outcome = _database.RunSuccessfully(
            "DECLARE @a VARCHAR(5) = 'one'; SELECT @a\nGO\nDECLARE @a VARCHAR(5) = 'two'; SELECT @a + '!'"
        );

        Assert.Equal(["one", "two!"], outcome.ResultSets.Select((ResultSet r) => r.Single()));
    }
}
