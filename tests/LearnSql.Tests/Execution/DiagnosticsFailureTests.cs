using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Execution;

public class DiagnosticsFailureTests
{
    public static TheoryData<string> ScriptsRejectedByTheDialectLayer() =>
        new TheoryData<string>
        {
            "SELECT 1 FROM",
            "SELEC 1",
            "PRAGMA table_info(Customer)",
            "PRAGMA foreign_keys = OFF",
            "VACUUM",
            "ATTACH DATABASE 'other.db' AS other",
            "DETACH DATABASE other",
            "SELECT * FROM Customer c CROSS APPLY (SELECT 1 AS x) a",
            "WHILE 1 = 0 SELECT 1",
            "PRINT 'x'",
            "SELECT @missing",
        };

    [Fact]
    public async Task ExecuteAsync_SyntaxError_ReturnsDiagnosticsErrorWithLineAndColumn()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync("SELECT 1;\nSELECT 2 FROM");

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal(ExecutionErrorKind.Diagnostics, result.Error.Kind);
        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(DiagnosticKind.ParseError, diagnostic.Kind);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(2, diagnostic.Span.Start.Line);
        Assert.Equal(14, diagnostic.Span.Start.Column);
        Assert.Equal(diagnostic.Span, result.Error.Origin);
        Assert.Equal(diagnostic.Message, result.Error.Message);
        Assert.Null(result.Error.MsSql);
        Assert.Null(result.Error.Sqlite);
    }

    [Fact]
    public async Task ExecuteAsync_UnsupportedConstruct_ReturnsDiagnosticsErrorWithPosition()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync("SELECT 1;\n  WHILE 1 = 0 SELECT 1");

        Assert.NotNull(result.Error);
        Assert.Equal(ExecutionErrorKind.Diagnostics, result.Error.Kind);
        DialectDiagnostic diagnostic = Assert.Single(
            result.Diagnostics,
            (DialectDiagnostic d) => d.Severity == DiagnosticSeverity.Error
        );
        Assert.Equal(DiagnosticKind.UnsupportedConstruct, diagnostic.Kind);
        Assert.Equal(2, diagnostic.Span.Start.Line);
        Assert.Equal(3, diagnostic.Span.Start.Column);
        Assert.False(string.IsNullOrWhiteSpace(result.Error.Detail));
    }

    [Fact]
    public async Task ExecuteAsync_TranslationError_ReturnsDiagnosticsErrorAtTheReference()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync("SELECT 1\nGO\nSELECT @missing");

        Assert.NotNull(result.Error);
        Assert.Equal(ExecutionErrorKind.Diagnostics, result.Error.Kind);
        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(DiagnosticKind.TranslationError, diagnostic.Kind);
        Assert.Equal(137, diagnostic.MsSqlErrorNumber);
        Assert.Equal(3, diagnostic.Span.Start.Line);
        Assert.Equal(8, diagnostic.Span.Start.Column);
    }

    [Theory]
    [MemberData(nameof(ScriptsRejectedByTheDialectLayer))]
    public async Task ExecuteAsync_DialectRejection_ExecutesNothingAndPositionsEveryError(
        string script
    )
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync(script);

        Assert.NotNull(result.Error);
        Assert.Equal(ExecutionErrorKind.Diagnostics, result.Error.Kind);
        Assert.Empty(result.Statements);
        Assert.False(result.RolledBack);
        Assert.NotNull(result.Error.Origin);
        Assert.All(
            result.Diagnostics.Where(
                (DialectDiagnostic d) => d.Severity == DiagnosticSeverity.Error
            ),
            (DialectDiagnostic d) =>
            {
                Assert.True(d.Span.Start.Line >= 1);
                Assert.True(d.Span.Start.Column >= 1);
            }
        );
    }

    [Fact]
    public async Task ExecuteAsync_ErrorLaterInTheScript_PreventsEarlierStatementsFromRunning()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync(
            "CREATE TABLE Early (a INT);\nINSERT INTO Early VALUES (1);\nSELECT 1 FROM"
        );

        Assert.Equal(ExecutionErrorKind.Diagnostics, result.Error?.Kind);
        DatabaseSchema schema = await database.GetSchemaAsync();
        Assert.Null(schema.Find("Early"));
    }

    [Fact]
    public async Task ExecuteAsync_PragmaVacuumAndAttach_FailBeforeReachingSqlite()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        foreach (
            string script in new[] { "PRAGMA writable_schema = 1", "VACUUM", "ATTACH 'x.db' AS x" }
        )
        {
            ExecutionResult result = await database.ExecuteAsync(script);

            ExecutionError error = result.ExpectError(ExecutionErrorKind.Diagnostics);
            Assert.Null(error.Sqlite);
        }
    }

    [Fact]
    public async Task ExecuteAsync_LoadExtension_IsDeniedByTheAuthorizer()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync("SELECT load_extension('nothing')");

        Assert.NotNull(result.Error);
        Assert.Equal(ExecutionErrorKind.Sql, result.Error.Kind);
        Assert.Contains("Not authorized to use function", result.Error.Message);
        Assert.Contains("load_extension", result.Error.Message);
    }

    [Fact]
    public async Task ExecuteAsync_LoadExtension_DoesNotBreakTheNextScript()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteAsync("SELECT load_extension('nothing')");

        ExecutionResult result = await database.ExecuteAsync("SELECT 'ok'");

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
    }

    [Fact]
    public async Task ExecuteAsync_ErrorResult_DisplaysTheMsSqlFormatForSqlErrorsAndTheMessageOtherwise()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT PRIMARY KEY)");

        ExecutionResult sqlError = await database.ExecuteAsync("INSERT INTO T VALUES (1), (1)");
        ExecutionResult syntaxError = await database.ExecuteAsync("SELECT FROM");

        Assert.NotNull(sqlError.Error);
        Assert.StartsWith("Msg ", sqlError.Error.Display);
        Assert.NotNull(syntaxError.Error);
        Assert.Equal(syntaxError.Error.Message, syntaxError.Error.Display);
    }
}
