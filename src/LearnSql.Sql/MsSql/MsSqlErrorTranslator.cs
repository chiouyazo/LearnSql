using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Exceptions;
using LearnSql.Sql.Generated;
using LearnSql.Sql.Models;
using LearnSql.Sql.Translation;
using Microsoft.Data.Sqlite;

namespace LearnSql.Sql.MsSql;

public sealed class MsSqlErrorTranslator : IErrorTranslator
{
    public const string ConstraintArgument = "Constraint";
    public const string StatementArgument = "Statement";
    public const string DatabaseArgument = "Database";
    public const string ContextTableArgument = "ContextTable";
    private const string DefaultStatement = "INSERT";

    public Language Language => Language.MsSql;

    public static IReadOnlyList<string> GetDerivedArgumentNames(SqliteErrorType type) =>
        type switch
        {
            SqliteErrorType.UniqueConstraintFailed or SqliteErrorType.PrimaryKeyConstraintFailed =>
            [
                ConstraintArgument,
            ],
            _ => [],
        };

    public MsSqlError Translate(
        SqliteError error,
        int? lineNumber = null,
        ErrorContext? context = null
    )
    {
        ArgumentNullException.ThrowIfNull(error);

        if (
            error.Type == SqliteErrorType.Unknown
            && MsSqlRaisedException.TryParse(
                error.Message,
                out MsSqlRaisedErrorKind raisedKind,
                out string raisedText
            )
        )
        {
            return MsSqlErrorCatalog.GetRaised(raisedKind).Create(raisedText, lineNumber);
        }

        if (!MsSqlErrorCatalog.TryGetDefinition(error.Type, out MsSqlErrorDefinition? definition))
        {
            return MsSqlErrorCatalog.CreateFallback(error.Message, lineNumber);
        }

        bool hasStatement = context is not null && context.StatementKind.IsDataModification();
        if (hasStatement && error.Type == SqliteErrorType.ForeignKeyConstraintFailed)
        {
            return definition.Create(ForeignKeyMessage.Build(context!), lineNumber);
        }

        Dictionary<string, string> arguments = BuildArguments(error, context);
        if (
            hasStatement
            && definition.ContextTemplate is string contextTemplate
            && MessageTemplate.TryRender(contextTemplate, arguments, out string contextMessage)
        )
        {
            return definition.Create(contextMessage, lineNumber);
        }

        if (MessageTemplate.TryRender(definition.Template, arguments, out string message))
        {
            return definition.Create(message, lineNumber);
        }

        return MsSqlErrorCatalog.CreateFallback(error.Message, lineNumber);
    }

    public TranslatedException CreateException(
        SqliteException exception,
        int? lineNumber = null,
        ErrorContext? context = null
    )
    {
        ArgumentNullException.ThrowIfNull(exception);

        SqliteError sqliteError = SqliteErrorClassifier.Classify(exception);
        return new TranslatedException(
            sqliteError,
            Translate(sqliteError, lineNumber, context),
            exception
        );
    }

    private static Dictionary<string, string> BuildArguments(
        SqliteError error,
        ErrorContext? context
    )
    {
        Dictionary<string, string> arguments = new Dictionary<string, string>(error.Arguments);
        arguments[StatementArgument] =
            context is not null && context.StatementKind.IsDataModification()
                ? context.StatementKind.ToKeyword()
                : DefaultStatement;
        arguments[DatabaseArgument] = SandboxSchemas.DatabaseName;
        if (context?.TargetTable is string targetTable)
        {
            arguments[ContextTableArgument] = targetTable;
        }

        switch (error.Type)
        {
            case SqliteErrorType.PrimaryKeyConstraintFailed:
                if (arguments.TryGetValue("Table", out string? primaryKeyTable))
                {
                    arguments[ConstraintArgument] = $"PK_{primaryKeyTable}";
                }

                break;
            case SqliteErrorType.UniqueConstraintFailed:
                if (
                    arguments.TryGetValue("Table", out string? uniqueTable)
                    && arguments.TryGetValue("Columns", out string? columns)
                )
                {
                    arguments[ConstraintArgument] =
                        $"UQ_{uniqueTable}_{string.Join('_', GetColumnNames(columns))}";
                }

                break;
        }

        return arguments;
    }

    private static string[] GetColumnNames(string columns) =>
        [
            .. columns
                .Split(',')
                .Select((string column) => column.Trim())
                .Select((string column) => column[(column.LastIndexOf('.') + 1)..])
                .Where((string column) => column.Length > 0),
        ];
}
