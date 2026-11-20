using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Execution;

public class SystemObjectPolicyTests
{
    public static TheoryData<string, string> ForbiddenModifications() =>
        new TheoryData<string, string>
        {
            { "DROP VIEW COLUMNS", "COLUMNS" },
            { "DROP VIEW TABLES", "TABLES" },
            { "CREATE TABLE TABLES (a INT)", "TABLES" },
            { "CREATE TABLE COLUMNS (a INT)", "COLUMNS" },
            { "DELETE FROM sqlite_sequence", "sqlite_sequence" },
            { "UPDATE sqlite_sequence SET seq = 0", "sqlite_sequence" },
            { "INSERT INTO sqlite_sequence VALUES ('x', 1)", "sqlite_sequence" },
            { "DROP TABLE __learnsql_variables", "__learnsql_variables" },
            { "DELETE FROM __learnsql_variables", "__learnsql_variables" },
        };

    [Theory]
    [MemberData(nameof(ForbiddenModifications))]
    public async Task ExecuteAsync_ModifyingASystemObject_IsRejected(
        string script,
        string objectName
    )
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );

        ExecutionResult result = await database.ExecuteAsync(script);

        Assert.NotNull(result.Error);
        Assert.Equal(ExecutionErrorKind.Rejected, result.Error.Kind);
        Assert.Equal(objectName, result.Error.Detail);
        Assert.Contains(objectName, result.Error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(result.Error.Origin);
        Assert.Empty(result.Statements);
    }

    [Fact]
    public async Task ExecuteAsync_RejectedStatement_ReportsItsOwnPositionInTheScript()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync("SELECT 1;\n\nDROP VIEW COLUMNS");

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Rejected);
        Assert.Equal(3, error.Origin?.Start.Line);
        Assert.Equal(1, error.Origin?.Start.Column);
    }

    [Theory]
    [InlineData("SELECT * FROM sqlite_master")]
    [InlineData("SELECT name FROM sqlite_master WHERE type = 'table'")]
    [InlineData("SELECT COUNT(*) FROM sqlite_sequence")]
    [InlineData("SELECT * FROM COLUMNS")]
    [InlineData("SELECT * FROM TABLES")]
    [InlineData("SELECT * FROM INFORMATION_SCHEMA.TABLES")]
    public async Task ExecuteAsync_ReadingSystemObjects_IsAllowed(string script)
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );

        ExecutionResult result = await database.ExecuteAsync(script);

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
        Assert.Single(result.ResultSets);
    }

    [Fact]
    public async Task ExecuteAsync_ReadingTheVariableTable_IsAllowedAfterADeclare()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("DECLARE @x INT = 3");

        ExecutionResult result = await database.ExecuteAsync("SELECT * FROM __learnsql_variables");

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
    }

    [Fact]
    public async Task ExecuteAsync_ViolationLateInTheScript_PreventsTheWholeScriptFromRunning()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync(
            "CREATE TABLE Early (a INT);\nINSERT INTO Early VALUES (1);\nDROP VIEW COLUMNS;"
        );

        Assert.Equal(ExecutionErrorKind.Rejected, result.Error?.Kind);
        Assert.Empty(result.Statements);
        DatabaseSchema schema = await database.GetSchemaAsync();
        Assert.Null(schema.Find("Early"));
    }

    [Fact]
    public async Task ExecuteAsync_ViolationInALaterBatch_PreventsTheEarlierBatchFromRunning()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync(
            "CREATE TABLE Early (a INT)\nGO\nDROP VIEW TABLES"
        );

        Assert.Equal(ExecutionErrorKind.Rejected, result.Error?.Kind);
        DatabaseSchema schema = await database.GetSchemaAsync();
        Assert.Null(schema.Find("Early"));
    }

    [Fact]
    public async Task ExecuteAsync_SystemViewsStayIntactAfterRejectedAttempts()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );
        await database.ExecuteAsync("DROP VIEW COLUMNS");
        await database.ExecuteAsync("DROP VIEW TABLES");

        object? tables = await database.ScalarAsync(
            "SELECT COUNT(*) FROM TABLES WHERE TABLE_NAME = 'Customer'"
        );

        Assert.Equal(1L, tables);
    }

    [Fact]
    public async Task ExecuteAsync_RejectionInRollbackMode_DoesNotMarkTheRunAsRolledBack()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync(
            "DROP VIEW COLUMNS",
            SandboxFactory.Rollback()
        );

        Assert.Equal(ExecutionErrorKind.Rejected, result.Error?.Kind);
        Assert.False(result.RolledBack);
    }
}
