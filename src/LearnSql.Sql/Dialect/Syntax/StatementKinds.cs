namespace LearnSql.Sql.Dialect.Syntax;

public static class StatementKinds
{
    public static bool IsDataModification(this StatementKind kind)
    {
        return kind is StatementKind.Insert or StatementKind.Update or StatementKind.Delete;
    }

    public static bool ChangesSchema(this StatementKind kind)
    {
        return kind
            is StatementKind.SelectInto
                or StatementKind.CreateTable
                or StatementKind.AlterTable
                or StatementKind.DropTable
                or StatementKind.CreateView
                or StatementKind.DropView
                or StatementKind.CreateIndex
                or StatementKind.DropIndex;
    }

    public static string ToKeyword(this StatementKind kind)
    {
        return kind switch
        {
            StatementKind.Insert => "INSERT",
            StatementKind.Update => "UPDATE",
            StatementKind.Delete => "DELETE",
            StatementKind.Select or StatementKind.SelectInto => "SELECT",
            StatementKind.TruncateTable => "TRUNCATE TABLE",
            StatementKind.CreateTable => "CREATE TABLE",
            StatementKind.AlterTable => "ALTER TABLE",
            StatementKind.DropTable => "DROP TABLE",
            StatementKind.CreateView => "CREATE VIEW",
            StatementKind.DropView => "DROP VIEW",
            StatementKind.CreateIndex => "CREATE INDEX",
            StatementKind.DropIndex => "DROP INDEX",
            StatementKind.BeginTransaction => "BEGIN TRANSACTION",
            StatementKind.CommitTransaction => "COMMIT TRANSACTION",
            StatementKind.RollbackTransaction => "ROLLBACK TRANSACTION",
            StatementKind.SaveTransaction => "SAVE TRANSACTION",
            StatementKind.DeclareVariable => "DECLARE",
            StatementKind.SetVariable => "SET",
            StatementKind.SetOption => "SET",
            _ => "statement",
        };
    }
}
