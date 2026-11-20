using LearnSql.Sql.Exceptions;
using LearnSql.Sql.Execution;
using LearnSql.Sql.Generated;
using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;
using LearnSql.Tests.Support;
using Microsoft.Data.Sqlite;

namespace LearnSql.Tests.Execution;

public class ErrorTranslationPipelineTests
{
    private const string Setup =
        "CREATE TABLE Parent (Id INT PRIMARY KEY, Code NVARCHAR(10) NOT NULL, Qty INT CHECK (Qty >= 0), "
        + "CONSTRAINT UQ_Parent_Code UNIQUE (Code));\n"
        + "CREATE TABLE Child (Id INT PRIMARY KEY, ParentId INT, "
        + "CONSTRAINT FK_Child_Parent FOREIGN KEY (ParentId) REFERENCES Parent (Id));\n"
        + "INSERT INTO Parent VALUES (1, 'a', 1), (2, 'b', 2);\n"
        + "INSERT INTO Child VALUES (1, 1);\n";

    private static async Task<SandboxDatabase> CreateAsync()
    {
        SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(Setup);
        return database;
    }

    private static async Task<ExecutionError> FailAsync(SandboxDatabase database, string script)
    {
        ExecutionResult result = await database.ExecuteAsync(script);
        Assert.NotNull(result.Error);
        Assert.Equal(ExecutionErrorKind.Sql, result.Error.Kind);
        Assert.NotNull(result.Error.MsSql);
        Assert.NotNull(result.Error.Sqlite);
        return result.Error;
    }

    [Fact]
    public async Task UniqueViolation_MapsToError2627WithTheConstraintName()
    {
        await using SandboxDatabase database = await CreateAsync();

        ExecutionError error = await FailAsync(database, "INSERT INTO Parent VALUES (3, 'a', 1)");

        Assert.Equal(2627, error.MsSql!.Number);
        Assert.Contains("UNIQUE KEY constraint 'UQ_Parent_Code'", error.Message);
        Assert.Contains("dbo.Parent", error.Message);
        Assert.Contains("UNIQUE constraint failed", error.Detail);
    }

    [Fact]
    public async Task PrimaryKeyViolation_MapsToError2627WithThePrimaryKeyName()
    {
        await using SandboxDatabase database = await CreateAsync();

        ExecutionError error = await FailAsync(database, "INSERT INTO Parent VALUES (1, 'z', 1)");

        Assert.Equal(2627, error.MsSql!.Number);
        Assert.Contains("PRIMARY KEY constraint 'PK_Parent'", error.Message);
    }

    [Fact]
    public async Task NotNullViolation_MapsToError515AndNamesTheStatement()
    {
        await using SandboxDatabase database = await CreateAsync();

        ExecutionError insert = await FailAsync(database, "INSERT INTO Parent VALUES (3, NULL, 1)");
        ExecutionError update = await FailAsync(
            database,
            "UPDATE Parent SET Code = NULL WHERE Id = 1"
        );

        Assert.Equal(515, insert.MsSql!.Number);
        Assert.Contains("column 'Code'", insert.Message);
        Assert.EndsWith("INSERT fails.", insert.Message);
        Assert.Equal(515, update.MsSql!.Number);
        Assert.EndsWith("UPDATE fails.", update.Message);
    }

    [Fact]
    public async Task CheckViolation_MapsToError547AndNamesTheStatement()
    {
        await using SandboxDatabase database = await CreateAsync();

        ExecutionError insert = await FailAsync(database, "INSERT INTO Parent VALUES (3, 'c', -1)");
        ExecutionError update = await FailAsync(database, "UPDATE Parent SET Qty = -5");

        Assert.Equal(547, insert.MsSql!.Number);
        Assert.StartsWith(
            "The INSERT statement conflicted with the CHECK constraint",
            insert.Message
        );
        Assert.Equal(547, update.MsSql!.Number);
        Assert.StartsWith(
            "The UPDATE statement conflicted with the CHECK constraint",
            update.Message
        );
    }

    [Fact]
    public async Task ForeignKeyViolationOnTheChildSide_MapsToError547WithTheConstraintAndParentColumn()
    {
        await using SandboxDatabase database = await CreateAsync();

        ExecutionError insert = await FailAsync(database, "INSERT INTO Child VALUES (2, 99)");
        ExecutionError update = await FailAsync(database, "UPDATE Child SET ParentId = 77");

        Assert.Equal(547, insert.MsSql!.Number);
        Assert.Equal(
            "The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_Child_Parent\". "
                + "The conflict occurred in database \"LearnSql\", table \"dbo.Parent\", column 'Id'.",
            insert.Message
        );
        Assert.Equal(547, update.MsSql!.Number);
        Assert.StartsWith(
            "The UPDATE statement conflicted with the FOREIGN KEY constraint \"FK_Child_Parent\"",
            update.Message
        );
    }

    [Fact]
    public async Task ForeignKeyViolationOnTheParentSide_MapsToAReferenceConstraintWithTheChildColumn()
    {
        await using SandboxDatabase database = await CreateAsync();

        ExecutionError delete = await FailAsync(database, "DELETE FROM Parent WHERE Id = 1");
        ExecutionError update = await FailAsync(database, "UPDATE Parent SET Id = 50 WHERE Id = 1");

        Assert.Equal(547, delete.MsSql!.Number);
        Assert.Equal(
            "The DELETE statement conflicted with the REFERENCE constraint \"FK_Child_Parent\". "
                + "The conflict occurred in database \"LearnSql\", table \"dbo.Child\", column 'ParentId'.",
            delete.Message
        );
        Assert.Equal(547, update.MsSql!.Number);
        Assert.StartsWith(
            "The UPDATE statement conflicted with the REFERENCE constraint \"FK_Child_Parent\"",
            update.Message
        );
        Assert.Contains("table \"dbo.Child\", column 'ParentId'", update.Message);
    }

    [Fact]
    public async Task ForeignKeyViolation_WithSeveralCandidateConstraints_OmitsTheConstraintName()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE A (Id INT PRIMARY KEY); CREATE TABLE B (Id INT PRIMARY KEY);"
                + "CREATE TABLE K (Id INT PRIMARY KEY, AId INT, BId INT, "
                + "CONSTRAINT FK_K_A FOREIGN KEY (AId) REFERENCES A (Id), "
                + "CONSTRAINT FK_K_B FOREIGN KEY (BId) REFERENCES B (Id));"
                + "INSERT INTO A VALUES (1); INSERT INTO B VALUES (1);"
        );

        ExecutionError error = await FailAsync(database, "INSERT INTO K VALUES (1, 5, 1)");

        Assert.Equal(547, error.MsSql!.Number);
        Assert.Equal(
            "The INSERT statement conflicted with the FOREIGN KEY constraint.",
            error.Message
        );
        Assert.DoesNotContain("FK_K_A", error.Message);
        Assert.DoesNotContain("FK_K_B", error.Message);
    }

    [Fact]
    public async Task ForeignKeyViolationOnADeleteWithSeveralChildTables_OmitsTheConstraintName()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE P (Id INT PRIMARY KEY);"
                + "CREATE TABLE C1 (Id INT PRIMARY KEY, PId INT, CONSTRAINT FK_C1 FOREIGN KEY (PId) REFERENCES P (Id));"
                + "CREATE TABLE C2 (Id INT PRIMARY KEY, PId INT, CONSTRAINT FK_C2 FOREIGN KEY (PId) REFERENCES P (Id));"
                + "INSERT INTO P VALUES (1); INSERT INTO C1 VALUES (1, 1);"
        );

        ExecutionError error = await FailAsync(database, "DELETE FROM P");

        Assert.Equal(
            "The DELETE statement conflicted with the REFERENCE constraint.",
            error.Message
        );
    }

    [Fact]
    public async Task ForeignKeyViolation_ThroughAnUnnamedConstraint_StillExplainsTheConflict()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE P (Id INT PRIMARY KEY); CREATE TABLE C (Id INT PRIMARY KEY, PId INT REFERENCES P (Id));"
        );

        ExecutionError error = await FailAsync(database, "INSERT INTO C VALUES (1, 5)");

        Assert.Equal(547, error.MsSql!.Number);
        Assert.StartsWith(
            "The INSERT statement conflicted with the FOREIGN KEY constraint",
            error.Message
        );
        Assert.Contains("table \"dbo.P\", column 'Id'", error.Message);
    }

    [Fact]
    public async Task LineNumber_IsRelativeToTheBatchWhileOriginIsAbsolute()
    {
        await using SandboxDatabase database = await CreateAsync();

        ExecutionError error = await FailAsync(
            database,
            "SELECT 1;\nSELECT 2;\nGO\nSELECT 3;\n\nINSERT INTO Parent VALUES (1, 'x', 1)"
        );

        Assert.Equal(3, error.MsSql!.LineNumber);
        Assert.Equal(6, error.Origin?.Start.Line);
        Assert.Equal(1, error.Origin?.Start.Column);
        Assert.StartsWith("Msg 2627, Level 14, State 1, Line 3", error.Display);
    }

    [Fact]
    public async Task LineNumber_InTheFirstBatch_EqualsTheAbsoluteLine()
    {
        await using SandboxDatabase database = await CreateAsync();

        ExecutionError error = await FailAsync(
            database,
            "\n\nINSERT INTO Parent VALUES (1, 'x', 1)"
        );

        Assert.Equal(3, error.MsSql!.LineNumber);
        Assert.Equal(3, error.Origin?.Start.Line);
    }

    [Fact]
    public async Task Error_KeepsTheResultsOfStatementsBeforeTheFailure()
    {
        await using SandboxDatabase database = await CreateAsync();

        ExecutionResult result = await database.ExecuteAsync(
            "SELECT COUNT(*) FROM Parent;\nINSERT INTO Parent VALUES (1, 'x', 1);\nSELECT 2;"
        );

        Assert.Equal(ExecutionErrorKind.Sql, result.Error?.Kind);
        StatementResult executed = Assert.Single(result.Statements);
        Assert.Equal(2L, executed.ResultSets[0].Rows[0][0]);
    }

    [Fact]
    public async Task Error_StopsTheScriptAtTheFailingStatement()
    {
        await using SandboxDatabase database = await CreateAsync();

        await database.ExecuteAsync(
            "INSERT INTO Parent VALUES (1, 'x', 1);\nINSERT INTO Parent VALUES (9, 'q', 1);"
        );

        Assert.Equal(2L, await database.CountRowsAsync("Parent"));
    }

    [Fact]
    public async Task Error_CarriesTheSqliteClassificationAndTheRawMessage()
    {
        await using SandboxDatabase database = await CreateAsync();

        ExecutionError error = await FailAsync(database, "INSERT INTO Parent VALUES (3, NULL, 1)");

        Assert.Equal(SqliteErrorType.NotNullConstraintFailed, error.Sqlite!.Type);
        Assert.Equal("SQLite Error 19: 'NOT NULL constraint failed: Parent.Code'.", error.Detail);
        Assert.Equal(error.MsSql!.ToString(), error.Display);
    }

    [Fact]
    public async Task UnknownTable_IsTranslatedToError208()
    {
        await using SandboxDatabase database = await CreateAsync();

        ExecutionError error = await FailAsync(database, "SELECT * FROM Missing");

        Assert.Equal(208, error.MsSql!.Number);
        Assert.Contains("Missing", error.Message);
    }

    [Fact]
    public async Task CommitWithoutTransaction_MapsToError3902()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionError error = await FailAsync(database, "COMMIT");

        Assert.Equal(3902, error.MsSql!.Number);
        Assert.Equal(1, error.MsSql.LineNumber);
        Assert.Equal(
            "The COMMIT TRANSACTION request has no corresponding BEGIN TRANSACTION.",
            error.Message
        );
    }

    [Fact]
    public async Task RollbackWithoutTransaction_MapsToError3903()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionError error = await FailAsync(database, "SELECT 1;\nROLLBACK");

        Assert.Equal(3903, error.MsSql!.Number);
        Assert.Equal(2, error.MsSql.LineNumber);
        Assert.Equal(
            "The ROLLBACK TRANSACTION request has no corresponding BEGIN TRANSACTION.",
            error.Message
        );
    }

    [Fact]
    public async Task CommitAfterAnEarlierCommit_MapsToError3902()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("BEGIN TRANSACTION; COMMIT;");

        ExecutionError error = await FailAsync(database, "COMMIT");

        Assert.Equal(3902, error.MsSql!.Number);
    }

    [Fact]
    public void Translator_WithoutErrorContext_KeepsTheLegacyWording()
    {
        MsSqlErrorTranslator translator = new MsSqlErrorTranslator();
        SqliteException foreignKey = SqliteFailure.Capture(
            SqliteFailure.ShopSetup,
            "INSERT INTO Child VALUES (1, 99, 1)"
        );
        SqliteException check = SqliteFailure.Capture(
            SqliteFailure.ShopSetup,
            "INSERT INTO Child VALUES (1, 1, 0)"
        );
        SqliteException notNull = SqliteFailure.Capture(
            SqliteFailure.ShopSetup,
            "INSERT INTO Child VALUES (1, NULL, 1)"
        );

        TranslatedException foreignKeyResult = translator.CreateException(foreignKey);
        TranslatedException checkResult = translator.CreateException(check);
        TranslatedException notNullResult = translator.CreateException(notNull);

        Assert.Equal(
            "The statement conflicted with the FOREIGN KEY constraint.",
            foreignKeyResult.Error.Message
        );
        Assert.Equal(
            "The statement conflicted with the CHECK constraint \"Qty > 0\".",
            checkResult.Error.Message
        );
        Assert.EndsWith("INSERT fails.", notNullResult.Error.Message);
    }
}
