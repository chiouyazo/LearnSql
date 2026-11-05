using LearnSql.Sql.Generated;
using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;

namespace LearnSql.Sql;

public static class ErrorTranslator
{
    public static GenericErrorMessages Current = new GenericErrorMessages();

    public static void SetLanguage(Language language)
    {
        if (language == Language.MsSql)
            Current = new MsSqlErrorMessages();
    }
}
