using LearnSql.Sql.Generated;

namespace LearnSql.Sql.Models;

public sealed record ErrorCoverage(int Mapped, int Total, IReadOnlyList<SqliteErrorType> Unmapped)
{
    public double Ratio => Total == 0 ? 0.0 : (double)Mapped / Total;
}
