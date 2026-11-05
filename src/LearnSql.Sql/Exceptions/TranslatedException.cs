using LearnSql.Sql.Generated;

namespace LearnSql.Sql.Exceptions;

public class TranslatedException : Exception
{
    public TranslatedException(SqliteError sqlError)
        : base(ErrorTranslator.Current.Format(sqlError)) { }
}
