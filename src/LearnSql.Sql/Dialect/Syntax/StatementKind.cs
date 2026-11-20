namespace LearnSql.Sql.Dialect.Syntax;

public enum StatementKind
{
    Other,
    Select,
    SelectInto,
    Insert,
    Update,
    Delete,
    TruncateTable,
    CreateTable,
    AlterTable,
    DropTable,
    CreateView,
    DropView,
    CreateIndex,
    DropIndex,
    BeginTransaction,
    CommitTransaction,
    RollbackTransaction,
    SaveTransaction,
    DeclareVariable,
    SetVariable,
    SetOption,
}
