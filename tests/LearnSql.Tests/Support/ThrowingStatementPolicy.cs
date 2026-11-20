using LearnSql.Sql.Dialect;
using LearnSql.Sql.Execution;

namespace LearnSql.Tests.Support;

public sealed class ThrowingStatementPolicy : IStatementPolicy
{
    public bool Throws { get; set; } = true;

    public int Evaluations { get; private set; }

    public PolicyViolation? Evaluate(TranslatedStatement statement)
    {
        Evaluations++;
        if (Throws)
        {
            throw new InvalidOperationException("The policy failed on purpose.");
        }

        return null;
    }
}
