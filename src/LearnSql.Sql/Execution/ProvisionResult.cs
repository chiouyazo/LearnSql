namespace LearnSql.Sql.Execution;

public sealed record ProvisionResult(string? ScenarioId, ProvisioningError? Error)
{
    public bool Succeeded => Error is null;
}
