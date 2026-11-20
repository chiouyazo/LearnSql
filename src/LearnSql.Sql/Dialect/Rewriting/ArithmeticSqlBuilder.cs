using LearnSql.Sql.Dialect.Functions;

namespace LearnSql.Sql.Dialect.Rewriting;

public static class ArithmeticSqlBuilder
{
    public static string Divide(string dividendSql, string divisorSql, bool divisorIsSafe = false)
    {
        string guardedDivisor = divisorIsSafe
            ? divisorSql
            : InternalFunctionNames.Divisor + "(" + divisorSql + ")";
        return dividendSql + " / " + guardedDivisor;
    }

    public static string Modulo(string dividendSql, string divisorSql)
    {
        return InternalFunctionNames.Modulo + "(" + dividendSql + ", " + divisorSql + ")";
    }
}
