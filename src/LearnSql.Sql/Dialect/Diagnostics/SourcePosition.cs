namespace LearnSql.Sql.Dialect.Diagnostics;

public readonly record struct SourcePosition(int Offset, int Line, int Column);
