using LearnSql.Sql.Dialect.Syntax;

namespace LearnSql.Core.Services.Exercises;

public static class DataChangeDetector
{
    public static bool ChangesDatabase(ExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return !result.RolledBack
            && result.Statements.Any(
                (StatementResult statement) =>
                    statement.Kind
                        is StatementKind.SelectInto
                            or StatementKind.Insert
                            or StatementKind.Update
                            or StatementKind.Delete
                            or StatementKind.TruncateTable
                            or StatementKind.CreateTable
                            or StatementKind.AlterTable
                            or StatementKind.DropTable
                            or StatementKind.CreateView
                            or StatementKind.DropView
                            or StatementKind.CreateIndex
                            or StatementKind.DropIndex
            );
    }
}
