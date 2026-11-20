using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;
using LearnSql.Sql.Translation;

namespace LearnSql.Tests.Translation;

public class ErrorTranslatorFactoryTests
{
    [Fact]
    public void Create_MsSql_ReturnsTheMsSqlTranslator()
    {
        IErrorTranslator translator = ErrorTranslatorFactory.Create(Language.MsSql);

        Assert.IsType<MsSqlErrorTranslator>(translator);
        Assert.Equal(Language.MsSql, translator.Language);
    }

    [Fact]
    public void Create_UndefinedLanguage_ThrowsArgumentOutOfRange()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ErrorTranslatorFactory.Create((Language)999)
        );

        Assert.Equal("language", exception.ParamName);
        Assert.Equal((Language)999, exception.ActualValue);
    }

    [Fact]
    public void Create_CalledTwice_ReturnsIndependentInstances()
    {
        IErrorTranslator first = ErrorTranslatorFactory.Create(Language.MsSql);
        IErrorTranslator second = ErrorTranslatorFactory.Create(Language.MsSql);

        Assert.NotSame(first, second);
    }
}
