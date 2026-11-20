using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Translation;

namespace LearnSql.Sql.MsSql;

public static class ForeignKeyMessage
{
    public const string ForeignKeyKind = "FOREIGN KEY";
    public const string ReferenceKind = "REFERENCE";

    public static string Build(ErrorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string statement = context.StatementKind.ToKeyword();
        string kind =
            context.StatementKind == StatementKind.Delete ? ReferenceKind : ForeignKeyKind;
        ForeignKeyConflict? conflict = context.ForeignKey;
        if (conflict is null)
        {
            return $"The {statement} statement conflicted with the {kind} constraint.";
        }

        bool parentSide = IsParentSide(context, conflict);
        string constraint = conflict.ConstraintName is null
            ? $"{(parentSide ? ReferenceKind : ForeignKeyKind)} constraint"
            : $"{(parentSide ? ReferenceKind : ForeignKeyKind)} constraint \"{conflict.ConstraintName}\"";
        string table = parentSide ? conflict.ChildTable : conflict.ParentTable;
        string column = parentSide ? conflict.ChildColumn : conflict.ParentColumn;
        return $"The {statement} statement conflicted with the {constraint}. "
            + $"The conflict occurred in database \"{SandboxSchemas.DatabaseName}\", table \"dbo.{table}\", column '{column}'.";
    }

    private static bool IsParentSide(ErrorContext context, ForeignKeyConflict conflict)
    {
        if (context.StatementKind == StatementKind.Delete)
        {
            return true;
        }

        return context.StatementKind == StatementKind.Update
            && string.Equals(
                context.TargetTable,
                conflict.ParentTable,
                StringComparison.OrdinalIgnoreCase
            )
            && !string.Equals(
                context.TargetTable,
                conflict.ChildTable,
                StringComparison.OrdinalIgnoreCase
            );
    }
}
