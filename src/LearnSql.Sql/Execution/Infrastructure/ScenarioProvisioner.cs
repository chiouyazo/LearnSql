using LearnSql.Data.Scenarios;
using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;
using LearnSql.Sql.Resources;
using LearnSql.Sql.Translation;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace LearnSql.Sql.Execution;

internal sealed class ScenarioProvisioner
{
    private readonly IErrorTranslator _translator;
    private readonly ICooperativeYield _yield;
    private readonly ILogger _logger;

    public ScenarioProvisioner(IErrorTranslator translator, ICooperativeYield yield, ILogger logger)
    {
        _translator = translator;
        _yield = yield;
        _logger = logger;
    }

    public async Task<ProvisioningError?> ApplyAsync(
        SandboxConnection connection,
        ScenarioScripts scripts,
        int firstStep,
        int totalSteps,
        IProgress<SandboxProgress>? progress,
        CancellationToken cancellationToken
    )
    {
        SqlScript? current = null;
        bool inTransaction = false;
        try
        {
            await connection.ExecuteNonQueryAsync("BEGIN", cancellationToken);
            inTransaction = true;

            (SandboxPhase Phase, IReadOnlyList<SqlScript> Scripts)[] groups =
            [
                (SandboxPhase.CreatingSchema, scripts.Schema),
                (SandboxPhase.CreatingViews, scripts.Views),
                (SandboxPhase.SeedingData, scripts.Seed),
            ];
            int step = firstStep;
            foreach ((SandboxPhase phase, IReadOnlyList<SqlScript> group) in groups)
            {
                foreach (SqlScript script in group)
                {
                    current = script;
                    progress?.Report(
                        new SandboxProgress(
                            phase,
                            step,
                            totalSteps,
                            ExecutionStrings.ProgressRunningScript(script.Resource)
                        )
                    );
                    await _yield.YieldAsync(cancellationToken);
                    await connection.ExecuteNonQueryAsync(script.Content, cancellationToken);
                    step++;
                }
            }

            await connection.ExecuteNonQueryAsync("COMMIT", cancellationToken);
            inTransaction = false;
            return null;
        }
        catch (OperationCanceledException)
        {
            await TryRollbackAsync(connection, inTransaction);
            return new ProvisioningError(
                ProvisioningErrorKind.Cancelled,
                ExecutionStrings.ProvisioningCancelled,
                current?.Resource
            );
        }
        catch (SqliteException exception)
        {
            await TryRollbackAsync(connection, inTransaction);
            SqliteError sqliteError = SqliteErrorClassifier.Classify(exception);
            MsSqlError translated = _translator.Translate(sqliteError);
            _logger.LogError(
                exception,
                "The scenario script {Resource} failed.",
                current?.Resource
            );
            return new ProvisioningError(
                ProvisioningErrorKind.ScriptFailed,
                ExecutionStrings.ScriptFailed(current?.Resource, translated.Message),
                current?.Resource,
                exception.Message,
                translated,
                sqliteError
            );
        }
    }

    private async Task TryRollbackAsync(SandboxConnection connection, bool inTransaction)
    {
        if (!inTransaction)
        {
            return;
        }

        try
        {
            await connection.ExecuteNonQueryAsync("ROLLBACK", CancellationToken.None);
        }
        catch (SqliteException exception)
        {
            _logger.LogWarning(exception, "The rollback after a failed scenario script failed.");
        }
    }
}
