namespace LearnSql.Sql.Execution;

public sealed class SandboxAuthorizationPolicy
{
    private const string InternalPrefix = "sqlite_";
    private const string SequenceTable = "sqlite_sequence";

    private static readonly string[] SchemaTables =
    [
        "sqlite_master",
        "sqlite_schema",
        "sqlite_temp_master",
        "sqlite_temp_schema",
    ];

    private static readonly string[] BlockedFunctions = ["load_extension", "fts3_tokenizer"];

    private static readonly string[] IntrospectionPragmas = ["table_info", "table_xinfo"];

    public AuthorizationDecision Decide(
        int actionCode,
        string? firstArgument,
        string? secondArgument
    )
    {
        return (AuthorizerAction)actionCode switch
        {
            AuthorizerAction.Attach or AuthorizerAction.Detach => AuthorizationDecision.Deny,
            AuthorizerAction.Pragma => DecidePragma(firstArgument),
            AuthorizerAction.Function => DecideFunction(secondArgument),
            AuthorizerAction.Insert or AuthorizerAction.Update => DecideWrite(firstArgument, false),
            AuthorizerAction.Delete => DecideWrite(firstArgument, true),
            _ => AuthorizationDecision.Allow,
        };
    }

    private static AuthorizationDecision DecidePragma(string? pragma)
    {
        return
            pragma is not null
            && IntrospectionPragmas.Contains(pragma, StringComparer.OrdinalIgnoreCase)
            ? AuthorizationDecision.Allow
            : AuthorizationDecision.Deny;
    }

    private static AuthorizationDecision DecideFunction(string? function)
    {
        return
            function is not null
            && BlockedFunctions.Contains(function, StringComparer.OrdinalIgnoreCase)
            ? AuthorizationDecision.Deny
            : AuthorizationDecision.Allow;
    }

    private static AuthorizationDecision DecideWrite(string? table, bool isDelete)
    {
        if (table is null || !table.StartsWith(InternalPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return AuthorizationDecision.Allow;
        }

        if (SchemaTables.Contains(table, StringComparer.OrdinalIgnoreCase))
        {
            return AuthorizationDecision.Allow;
        }

        bool sequenceCleanup =
            isDelete && string.Equals(table, SequenceTable, StringComparison.OrdinalIgnoreCase);
        return sequenceCleanup ? AuthorizationDecision.Allow : AuthorizationDecision.Deny;
    }
}
