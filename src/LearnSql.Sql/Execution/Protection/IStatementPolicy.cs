using LearnSql.Sql.Dialect;

namespace LearnSql.Sql.Execution;

public interface IStatementPolicy
{
    PolicyViolation? Evaluate(TranslatedStatement statement);
}
