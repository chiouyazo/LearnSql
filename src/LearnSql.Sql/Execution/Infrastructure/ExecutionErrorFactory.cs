using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;
using LearnSql.Sql.Resources;
using LearnSql.Sql.Translation;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace LearnSql.Sql.Execution;

internal sealed class ExecutionErrorFactory
{
    private const int SqliteInterruptCode = 9;
    private const int SqliteFullCode = 13;
    private const int SqliteTooBigCode = 18;

    private readonly IErrorTranslator _translator;
    private readonly ForeignKeyConflictResolver _resolver;
    private readonly ILogger _logger;

    public ExecutionErrorFactory(
        IErrorTranslator translator,
        ForeignKeyConflictResolver resolver,
        ILogger logger
    )
    {
        _translator = translator;
        _resolver = resolver;
        _logger = logger;
    }

    public ExecutionError FromDiagnostics(IReadOnlyList<DialectDiagnostic> diagnostics)
    {
        DialectDiagnostic first = diagnostics.First(
            (DialectDiagnostic diagnostic) => diagnostic.Severity == DiagnosticSeverity.Error
        );
        return new ExecutionError(
            ExecutionErrorKind.Diagnostics,
            first.Message,
            first.Hint,
            Origin: first.Span
        );
    }

    public ExecutionError FromViolation(PolicyViolation violation)
    {
        return new ExecutionError(
            ExecutionErrorKind.Rejected,
            violation.Message,
            violation.ObjectName,
            Origin: violation.Statement.Origin
        );
    }

    public ExecutionError FromFailure(
        Exception exception,
        FailureSite? site,
        GuardAbortReason reason,
        TimeSpan timeout,
        Func<DatabaseSchema> schema
    )
    {
        SourceSpan? origin = site?.Statement.Origin;
        switch (exception)
        {
            case OperationCanceledException:
                return Cancelled(origin);
            case ExecutionTimeoutException timeoutException:
                return Timeout(timeoutException.Limit, origin);
            case SqliteException sqlite:
                return FromSqlite(sqlite, site, reason, timeout, schema);
            default:
                _logger.LogError(
                    exception,
                    "Unexpected failure while executing a statement of kind {Kind}.",
                    site?.Statement.EffectiveProfile.Kind
                );
                return new ExecutionError(
                    ExecutionErrorKind.Internal,
                    ExecutionStrings.UnexpectedExecutionError,
                    $"{exception.GetType().Name}: {exception.Message}",
                    Origin: origin
                );
        }
    }

    private ExecutionError FromSqlite(
        SqliteException exception,
        FailureSite? site,
        GuardAbortReason reason,
        TimeSpan timeout,
        Func<DatabaseSchema> schema
    )
    {
        SourceSpan? origin = site?.Statement.Origin;
        if (reason == GuardAbortReason.Cancelled)
        {
            return Cancelled(origin);
        }

        if (reason == GuardAbortReason.TimedOut)
        {
            return Timeout(timeout, origin);
        }

        if (exception.SqliteErrorCode is SqliteFullCode or SqliteTooBigCode)
        {
            return new ExecutionError(
                ExecutionErrorKind.LimitExceeded,
                ExecutionStrings.StorageLimitReached,
                exception.Message,
                Origin: origin
            );
        }

        if (exception.SqliteErrorCode == SqliteInterruptCode)
        {
            _logger.LogWarning(
                exception,
                "SQLite reported an interrupt that was not requested by the sandbox."
            );
        }

        SqliteError sqliteError = SqliteErrorClassifier.Classify(exception);
        ErrorContext? context = BuildContext(sqliteError, site, schema);
        MsSqlError translated =
            (
                site is null
                    ? null
                    : MsSqlTransactionErrors.TryCreate(
                        site.Statement.EffectiveProfile.Kind,
                        exception.Message,
                        site.RelativeLine
                    )
            ) ?? _translator.Translate(sqliteError, site?.RelativeLine, context);
        return new ExecutionError(
            ExecutionErrorKind.Sql,
            translated.Message,
            exception.Message,
            translated,
            sqliteError,
            origin
        );
    }

    private ErrorContext? BuildContext(
        SqliteError error,
        FailureSite? site,
        Func<DatabaseSchema> schema
    )
    {
        if (site is null)
        {
            return null;
        }

        StatementProfile profile = site.Statement.EffectiveProfile;
        ForeignKeyConflict? conflict = null;
        if (error.Type == Generated.SqliteErrorType.ForeignKeyConstraintFailed)
        {
            conflict = TryResolveConflict(profile, schema);
        }

        return new ErrorContext(profile.Kind, profile.TargetTable, conflict);
    }

    private ForeignKeyConflict? TryResolveConflict(
        StatementProfile profile,
        Func<DatabaseSchema> schema
    )
    {
        try
        {
            return _resolver.Resolve(schema(), profile.Kind, profile.TargetTable);
        }
        catch (Exception exception) when (exception is SqliteException or InvalidOperationException)
        {
            _logger.LogWarning(exception, "The foreign key could not be resolved from the schema.");
            return null;
        }
    }

    public static ExecutionError WithTransactionNote(ExecutionError error)
    {
        return error with
        {
            Message = error.Message + " " + ExecutionStrings.TransactionRolledBack,
        };
    }

    private static ExecutionError Cancelled(SourceSpan? origin)
    {
        return new ExecutionError(
            ExecutionErrorKind.Cancelled,
            ExecutionStrings.Cancelled,
            Origin: origin
        );
    }

    private static ExecutionError Timeout(TimeSpan limit, SourceSpan? origin)
    {
        return new ExecutionError(
            ExecutionErrorKind.Timeout,
            ExecutionStrings.TimeLimitExceeded(limit),
            Origin: origin
        );
    }
}
