using LearnSql.Sql.Generated;
using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;

namespace LearnSql.Tests.MsSql;

public class MsSqlErrorCatalogTests
{
    public static TheoryData<SqliteErrorType> MappedTypes()
    {
        TheoryData<SqliteErrorType> data = new TheoryData<SqliteErrorType>();
        foreach (SqliteErrorType type in MsSqlErrorCatalog.MappedTypes.Order())
            data.Add(type);
        return data;
    }

    [Fact]
    public void GetCoverage_MappedPlusUnmapped_EqualsTotal()
    {
        ErrorCoverage coverage = MsSqlErrorCatalog.GetCoverage();

        Assert.Equal(coverage.Total, coverage.Mapped + coverage.Unmapped.Count);
    }

    [Fact]
    public void GetCoverage_TotalIsEveryEnumValueExceptUnknown()
    {
        ErrorCoverage coverage = MsSqlErrorCatalog.GetCoverage();

        Assert.Equal(Enum.GetValues<SqliteErrorType>().Length - 1, coverage.Total);
    }

    [Fact]
    public void GetCoverage_MapsAtLeastTheTwentyTwoDocumentedTypes()
    {
        ErrorCoverage coverage = MsSqlErrorCatalog.GetCoverage();

        Assert.True(coverage.Mapped >= 22, $"Only {coverage.Mapped} types are mapped.");
    }

    [Fact]
    public void GetCoverage_MappedCountEqualsTheCatalogSize()
    {
        ErrorCoverage coverage = MsSqlErrorCatalog.GetCoverage();

        Assert.Equal(MsSqlErrorCatalog.MappedTypes.Count, coverage.Mapped);
    }

    [Fact]
    public void GetCoverage_UnmappedTypes_AreNeitherUnknownNorMapped()
    {
        ErrorCoverage coverage = MsSqlErrorCatalog.GetCoverage();

        Assert.DoesNotContain(SqliteErrorType.Unknown, coverage.Unmapped);
        Assert.Empty(coverage.Unmapped.Intersect(MsSqlErrorCatalog.MappedTypes));
        Assert.Equal(coverage.Unmapped.Count, coverage.Unmapped.Distinct().Count());
    }

    [Fact]
    public void GetCoverage_Ratio_IsMappedDividedByTotal()
    {
        ErrorCoverage coverage = MsSqlErrorCatalog.GetCoverage();

        Assert.Equal((double)coverage.Mapped / coverage.Total, coverage.Ratio);
    }

    [Fact]
    public void ErrorCoverage_WithZeroTotal_HasZeroRatio()
    {
        ErrorCoverage coverage = new ErrorCoverage(0, 0, []);

        Assert.Equal(0.0, coverage.Ratio);
    }

    [Fact]
    public void MappedTypes_DoNotContainUnknown()
    {
        Assert.DoesNotContain(SqliteErrorType.Unknown, MsSqlErrorCatalog.MappedTypes);
    }

    [Fact]
    public void TryGetDefinition_UnknownType_ReturnsFalse()
    {
        bool found = MsSqlErrorCatalog.TryGetDefinition(
            SqliteErrorType.Unknown,
            out MsSqlErrorDefinition? definition
        );

        Assert.False(found);
        Assert.Null(definition);
    }

    [Theory]
    [MemberData(nameof(MappedTypes))]
    public void Definition_OfMappedType_HasPlausibleNumberSeverityAndState(SqliteErrorType type)
    {
        MsSqlErrorCatalog.TryGetDefinition(type, out MsSqlErrorDefinition? definition);

        Assert.NotNull(definition);
        Assert.InRange(definition.Number, 100, 49999);
        Assert.InRange(definition.Severity, 1, 25);
        Assert.InRange(definition.State, 0, 255);
        Assert.False(string.IsNullOrWhiteSpace(definition.Template));
    }

    [Theory]
    [MemberData(nameof(MappedTypes))]
    public void Template_OfMappedType_UsesOnlyDeclaredOrDerivedArguments(SqliteErrorType type)
    {
        MsSqlErrorCatalog.TryGetDefinition(type, out MsSqlErrorDefinition? definition);
        HashSet<string> allowed =
        [
            .. SqliteErrorArguments.GetNames(type),
            .. MsSqlErrorTranslator.GetDerivedArgumentNames(type),
        ];

        IReadOnlyList<string> placeholders = MessageTemplate.GetPlaceholders(definition!.Template);

        Assert.All(
            placeholders,
            (string placeholder) =>
                Assert.True(
                    allowed.Contains(placeholder),
                    $"Placeholder '{placeholder}' of {type} is never provided."
                )
        );
    }

    [Theory]
    [MemberData(nameof(MappedTypes))]
    public void Template_OfMappedType_HasBalancedBraces(SqliteErrorType type)
    {
        MsSqlErrorCatalog.TryGetDefinition(type, out MsSqlErrorDefinition? definition);
        string template = definition!.Template;

        int opening = template.Count((char c) => c == '{');
        int closing = template.Count((char c) => c == '}');

        Assert.Equal(opening, closing);
    }

    [Theory]
    [InlineData(SqliteErrorType.NoSuchTable, 208, 16, 1)]
    [InlineData(SqliteErrorType.NoSuchColumn, 207, 16, 1)]
    [InlineData(SqliteErrorType.AmbiguousColumnName, 209, 16, 1)]
    [InlineData(SqliteErrorType.NearSyntaxError, 102, 15, 1)]
    [InlineData(SqliteErrorType.NoSuchFunction, 195, 15, 10)]
    [InlineData(SqliteErrorType.ValuesForColumns, 213, 16, 1)]
    [InlineData(SqliteErrorType.AlreadyExists, 2714, 16, 6)]
    [InlineData(SqliteErrorType.UniqueConstraintFailed, 2627, 14, 1)]
    [InlineData(SqliteErrorType.PrimaryKeyConstraintFailed, 2627, 14, 1)]
    [InlineData(SqliteErrorType.NotNullConstraintFailed, 515, 16, 2)]
    [InlineData(SqliteErrorType.ForeignKeyConstraintFailed, 547, 16, 0)]
    [InlineData(SqliteErrorType.CheckConstraintFailed, 547, 16, 0)]
    [InlineData(SqliteErrorType.NoTablesSpecified, 1038, 15, 1)]
    [InlineData(SqliteErrorType.NoSuchIndex, 3701, 11, 5)]
    public void Definition_OfDocumentedType_MatchesTheSqlServerErrorNumberSeverityAndState(
        SqliteErrorType type,
        int number,
        int severity,
        int state
    )
    {
        MsSqlErrorCatalog.TryGetDefinition(type, out MsSqlErrorDefinition? definition);

        Assert.NotNull(definition);
        Assert.Equal(number, definition.Number);
        Assert.Equal(severity, definition.Severity);
        Assert.Equal(state, definition.State);
    }

    [Fact]
    public void Fallback_Constants_AreMsg50000Level16State1()
    {
        Assert.Equal(50000, MsSqlErrorCatalog.FallbackNumber);
        Assert.Equal(16, MsSqlErrorCatalog.FallbackSeverity);
        Assert.Equal(1, MsSqlErrorCatalog.FallbackState);
    }

    [Theory]
    [InlineData("disk I/O error", "Disk I/O error.")]
    [InlineData("  already trimmed.  ", "Already trimmed.")]
    [InlineData("Ends with question?", "Ends with question?")]
    [InlineData("Ends with bang!", "Ends with bang!")]
    [InlineData("x", "X.")]
    public void CreateFallback_FormatsTheOriginalMessageAsASentence(
        string original,
        string expected
    )
    {
        MsSqlError error = MsSqlErrorCatalog.CreateFallback(original);

        Assert.Equal(50000, error.Number);
        Assert.Equal(16, error.Severity);
        Assert.Equal(1, error.State);
        Assert.Equal(expected, error.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateFallback_EmptyOrMissingMessage_UsesTheGenericText(string? original)
    {
        MsSqlError error = MsSqlErrorCatalog.CreateFallback(original!);

        Assert.Equal(MsSqlErrorCatalog.FallbackMessage, error.Message);
    }

    [Fact]
    public void CreateFallback_WithLineNumber_StoresTheLine()
    {
        MsSqlError error = MsSqlErrorCatalog.CreateFallback("oops", 7);

        Assert.Equal(7, error.LineNumber);
        Assert.Equal("Msg 50000, Level 16, State 1, Line 7", error.Header);
    }
}
