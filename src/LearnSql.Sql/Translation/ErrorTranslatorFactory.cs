using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;

namespace LearnSql.Sql.Translation;

public static class ErrorTranslatorFactory
{
    public static IErrorTranslator Create(Language language) =>
        language switch
        {
            Language.MsSql => new MsSqlErrorTranslator(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(language),
                language,
                $"No error translator is registered for language '{language}'."
            ),
        };
}
