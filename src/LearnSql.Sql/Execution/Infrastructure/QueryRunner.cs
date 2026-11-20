using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Syntax;
using Microsoft.Extensions.Logging;

namespace LearnSql.Sql.Execution;

internal sealed class QueryRunner
{
    private readonly IReadOnlyList<IStatementPolicy> _policies;
    private readonly SandboxAuthorizationPolicy _authorization;
    private readonly ICooperativeYield _yield;
    private readonly StatementRunner _statementRunner;
    private readonly ExecutionErrorFactory _errors;
    private readonly SandboxOptions _sandboxOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;

    public QueryRunner(
        IReadOnlyList<IStatementPolicy> policies,
        SandboxAuthorizationPolicy authorization,
        ICooperativeYield yield,
        StatementRunner statementRunner,
        ExecutionErrorFactory errors,
        SandboxOptions sandboxOptions,
        TimeProvider timeProvider,
        ILogger logger
    )
    {
        _policies = policies;
        _authorization = authorization;
        _yield = yield;
        _statementRunner = statementRunner;
        _errors = errors;
        _sandboxOptions = sandboxOptions;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<QueryRunOutcome> RunAsync(
        string tsql,
        SandboxConnection connection,
        TSqlDialectTranslator translator,
        ExecutionOptions options,
        Func<DatabaseSchema> schema,
        CancellationToken cancellationToken
    )
    {
        long start = _timeProvider.GetTimestamp();
        _yield.StartSlice();

        DialectTranslationResult translation;
        try
        {
            translation = translator.Translate(tsql);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            ExecutionError error = _errors.FromFailure(
                exception,
                null,
                GuardAbortReason.None,
                options.Timeout,
                schema
            );
            return Finish([], [], error, start, false, false);
        }

        if (!translation.Success)
        {
            return Finish(
                [],
                translation.Diagnostics,
                _errors.FromDiagnostics(translation.Diagnostics),
                start,
                false,
                false
            );
        }

        PolicyViolation? violation = FindViolation(translation);
        if (violation is not null)
        {
            return Finish(
                [],
                translation.Diagnostics,
                _errors.FromViolation(violation),
                start,
                false,
                false
            );
        }

        return await ExecuteAsync(
            translation,
            connection,
            options,
            schema,
            start,
            cancellationToken
        );
    }

    private async Task<QueryRunOutcome> ExecuteAsync(
        DialectTranslationResult translation,
        SandboxConnection connection,
        ExecutionOptions options,
        Func<DatabaseSchema> schema,
        long start,
        CancellationToken cancellationToken
    )
    {
        ITransactionStrategy transaction = options.RollbackAfterExecution
            ? new RollbackTransactionStrategy(connection)
            : new PassThroughTransactionStrategy();
        List<StatementResult> results = [];
        Exception? failure = null;
        FailureSite? site = null;
        bool schemaMayHaveChanged = false;
        bool transactionStarted = false;
        GuardAbortReason reason = GuardAbortReason.None;
        bool transactionWasOpen = !connection.IsAutocommit;

        using (
            ExecutionGuard guard = new ExecutionGuard(
                connection,
                options.Timeout,
                cancellationToken,
                _timeProvider,
                _authorization,
                _logger
            )
        )
        {
            try
            {
                await transaction.BeginAsync(cancellationToken);
                transactionStarted = true;
                guard.Install(_sandboxOptions.ProgressHandlerInstructions);

                for (int batchIndex = 0; batchIndex < translation.Batches.Count; batchIndex++)
                {
                    TranslatedBatch batch = translation.Batches[batchIndex];
                    for (int iteration = 0; iteration < batch.RepeatCount; iteration++)
                    {
                        foreach (TranslatedStatement statement in batch.Statements)
                        {
                            site = new FailureSite(
                                statement,
                                statement.Origin.Start.Line - batch.Span.Start.Line + 1
                            );
                            await _yield.YieldIfDueAsync(cancellationToken);
                            guard.Poll();

                            StatementProfile profile = statement.EffectiveProfile;
                            transactionWasOpen |= profile.Kind == StatementKind.BeginTransaction;
                            schemaMayHaveChanged |=
                                profile.ChangesSchema
                                || profile.Kind == StatementKind.RollbackTransaction;
                            StatementResult result = await _statementRunner.RunAsync(
                                connection,
                                statement,
                                transaction.Resolve(statement),
                                batchIndex,
                                iteration,
                                options,
                                guard,
                                cancellationToken
                            );
                            if (!statement.IsGenerated)
                            {
                                results.Add(result);
                            }
                        }
                    }
                }

                site = null;
            }
            catch (Exception exception)
            {
                failure = exception;
                schemaMayHaveChanged = true;
            }

            reason = guard.Reason;
        }

        ExecutionError? error = null;
        if (failure is not null)
        {
            error = _errors.FromFailure(failure, site, reason, options.Timeout, schema);
            bool interrupted =
                error.Kind is ExecutionErrorKind.Timeout or ExecutionErrorKind.Cancelled;
            if (
                interrupted
                && transactionWasOpen
                && !transaction.RollsBack
                && connection.IsAutocommit
            )
            {
                error = ExecutionErrorFactory.WithTransactionNote(error);
            }
        }

        ExecutionError? endError = await TryEndTransactionAsync(transaction);
        error ??= endError;

        return Finish(
            results,
            translation.Diagnostics,
            error,
            start,
            transaction.RollsBack && transactionStarted,
            schemaMayHaveChanged || transaction.RollsBack
        );
    }

    private async Task<ExecutionError?> TryEndTransactionAsync(ITransactionStrategy transaction)
    {
        try
        {
            await transaction.EndAsync();
            return null;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return _errors.FromFailure(
                exception,
                null,
                GuardAbortReason.None,
                Timeout.InfiniteTimeSpan,
                () => DatabaseSchema.Empty
            );
        }
    }

    private PolicyViolation? FindViolation(DialectTranslationResult translation)
    {
        foreach (TranslatedBatch batch in translation.Batches)
        {
            foreach (TranslatedStatement statement in batch.Statements)
            {
                foreach (IStatementPolicy policy in _policies)
                {
                    PolicyViolation? violation = policy.Evaluate(statement);
                    if (violation is not null)
                    {
                        return violation;
                    }
                }
            }
        }

        return null;
    }

    private QueryRunOutcome Finish(
        IReadOnlyList<StatementResult> statements,
        IReadOnlyList<DialectDiagnostic> diagnostics,
        ExecutionError? error,
        long start,
        bool rolledBack,
        bool schemaMayHaveChanged
    )
    {
        ExecutionResult result = new ExecutionResult(
            statements,
            diagnostics,
            error,
            _timeProvider.GetElapsedTime(start),
            rolledBack
        );
        return new QueryRunOutcome(result, schemaMayHaveChanged);
    }
}
