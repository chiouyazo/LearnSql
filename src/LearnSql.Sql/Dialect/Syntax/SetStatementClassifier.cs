using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Syntax;

public static class SetStatementClassifier
{
    private const SetOptions AlwaysIgnorable =
        SetOptions.NoCount
        | SetOptions.XactAbort
        | SetOptions.ArithAbort
        | SetOptions.NumericRoundAbort;

    private const SetOptions IgnorableWhenOn =
        SetOptions.QuotedIdentifier
        | SetOptions.AnsiNulls
        | SetOptions.AnsiPadding
        | SetOptions.AnsiWarnings
        | SetOptions.ConcatNullYieldsNull;

    public static bool IsIgnorable(TSqlFragment fragment)
    {
        switch (fragment)
        {
            case PredicateSetStatement predicate:
                return IsIgnorable(predicate);
            case SetTransactionIsolationLevelStatement:
                return true;
            default:
                return false;
        }
    }

    public static bool IsIgnorable(PredicateSetStatement statement)
    {
        if (statement.Options == SetOptions.None)
            return false;

        SetOptions allowed = statement.IsOn ? AlwaysIgnorable | IgnorableWhenOn : AlwaysIgnorable;
        return (statement.Options & ~allowed) == SetOptions.None;
    }
}
