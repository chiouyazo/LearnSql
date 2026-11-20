using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Syntax;

public static class TableHintClassifier
{
    private static readonly HashSet<TableHintKind> IgnorableHints =
    [
        TableHintKind.NoLock,
        TableHintKind.ReadUncommitted,
        TableHintKind.ReadCommitted,
        TableHintKind.ReadCommittedLock,
        TableHintKind.RepeatableRead,
        TableHintKind.Serializable,
        TableHintKind.Snapshot,
        TableHintKind.HoldLock,
        TableHintKind.NoWait,
        TableHintKind.ReadPast,
        TableHintKind.UpdLock,
        TableHintKind.XLock,
        TableHintKind.Rowlock,
        TableHintKind.PagLock,
        TableHintKind.TabLock,
        TableHintKind.TabLockX,
    ];

    public static bool IsIgnorable(TableHint hint)
    {
        return IgnorableHints.Contains(hint.HintKind);
    }
}
