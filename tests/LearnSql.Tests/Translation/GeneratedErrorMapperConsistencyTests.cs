using LearnSql.Sql.Generated;
using LearnSql.Sql.Models;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Translation;

public class GeneratedErrorMapperConsistencyTests
{
    private static readonly SqliteErrorType[] AllTypes = [.. Enum.GetValues<SqliteErrorType>()];

    public static TheoryData<SqliteErrorType> MatchableTypes()
    {
        TheoryData<SqliteErrorType> data = new TheoryData<SqliteErrorType>();
        foreach (SqliteErrorType type in SqliteErrorMapper.MatchableTypes)
            data.Add(type);
        return data;
    }

    [Fact]
    public void MatchableTypes_ContainEveryEnumValueExceptUnknownAndClassifierOnlyTypes_ExactlyOnce()
    {
        SqliteErrorType[] expected =
        [
            .. AllTypes.Where(
                (SqliteErrorType type) =>
                    type != SqliteErrorType.Unknown
                    && type != SqliteErrorType.PrimaryKeyConstraintFailed
            ),
        ];

        Assert.Equal(expected.Order(), SqliteErrorMapper.MatchableTypes.Order());
        Assert.Equal(
            SqliteErrorMapper.MatchableTypes.Count,
            SqliteErrorMapper.MatchableTypes.Distinct().Count()
        );
    }

    [Fact]
    public void PrimaryKeyConstraintFailed_IsOnlyProducedByTheClassifier()
    {
        Assert.DoesNotContain(
            SqliteErrorType.PrimaryKeyConstraintFailed,
            SqliteErrorMapper.MatchableTypes
        );
    }

    [Fact]
    public void MatchableTypes_DoNotContainUnknown()
    {
        Assert.DoesNotContain(SqliteErrorType.Unknown, SqliteErrorMapper.MatchableTypes);
    }

    [Theory]
    [MemberData(nameof(MatchableTypes))]
    public void Match_ExampleMessageOfRule_ResolvesToExactlyThatType(SqliteErrorType type)
    {
        ErrorMessageExample example = ErrorMessageExample.Create(type);

        SqliteError result = SqliteErrorMapper.Match(example.Message);

        Assert.Equal(type, result.Type);
    }

    [Theory]
    [MemberData(nameof(MatchableTypes))]
    public void Match_ExampleMessageOfRule_ExtractsTheCapturedArguments(SqliteErrorType type)
    {
        ErrorMessageExample example = ErrorMessageExample.Create(type);

        SqliteError result = SqliteErrorMapper.Match(example.Message);

        if (example.HasAdjacentGroups)
        {
            Assert.Equal(example.Arguments.Keys.Order(), result.Arguments.Keys.Order());
            return;
        }

        Assert.Equal(
            example.Arguments.OrderBy((KeyValuePair<string, string> pair) => pair.Key),
            result.Arguments.OrderBy((KeyValuePair<string, string> pair) => pair.Key)
        );
    }

    [Theory]
    [MemberData(nameof(MatchableTypes))]
    public void Match_ExampleMessageInWrapperFormat_ResolvesToTheSameTypeAndArguments(
        SqliteErrorType type
    )
    {
        ErrorMessageExample example = ErrorMessageExample.Create(type);
        string wrapped = $"SQLite Error 1: '{example.Message}'.";

        SqliteError result = SqliteErrorMapper.Match(wrapped);

        Assert.Equal(type, result.Type);
        Assert.Equal(example.Message, result.Message);
    }

    [Theory]
    [MemberData(nameof(MatchableTypes))]
    public void ArgumentNames_OfRule_CoverEveryCapturedArgument(SqliteErrorType type)
    {
        ErrorMessageExample example = ErrorMessageExample.Create(type);

        SqliteError result = SqliteErrorMapper.Match(example.Message);

        IReadOnlyList<string> declared = SqliteErrorArguments.GetNames(type);
        Assert.Equal(declared.Order(), result.Arguments.Keys.Order());
    }

    [Fact]
    public void ArgumentNames_OfUnknown_AreEmpty()
    {
        Assert.Empty(SqliteErrorArguments.GetNames(SqliteErrorType.Unknown));
    }
}
