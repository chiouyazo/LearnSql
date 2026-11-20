namespace LearnSql.Sql.Dialect.Functions;

public readonly record struct SqlDateValue(DateTime Value, bool IsDateOnly, int FractionDigits);
