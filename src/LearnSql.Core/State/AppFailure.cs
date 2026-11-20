namespace LearnSql.Core.State;

public sealed record AppFailure(AppFailureKind Kind, string Detail);
