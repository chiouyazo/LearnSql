namespace LearnSql.Sql.Dialect.Batching;

public sealed record SqlBatchSource(string Text, int StartOffset, int RepeatCount);
