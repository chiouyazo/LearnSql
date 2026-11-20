using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using LearnSql.Sql.Generated;
using LearnSql.Sql.Models;

namespace LearnSql.Sql.MsSql;

public static class MsSqlErrorCatalog
{
    public const int FallbackNumber = 50000;
    public const int FallbackSeverity = 16;
    public const int FallbackState = 1;
    public const string FallbackMessage = "An unspecified error occurred.";

    private static readonly MsSqlErrorDefinition InvalidObjectName = new MsSqlErrorDefinition(
        208,
        16,
        1,
        "Invalid object name '{Table}'."
    );

    private static readonly MsSqlErrorDefinition InvalidColumnName = new MsSqlErrorDefinition(
        207,
        16,
        1,
        "Invalid column name '{Column}'."
    );

    private static readonly MsSqlErrorDefinition AmbiguousColumnName = new MsSqlErrorDefinition(
        209,
        16,
        1,
        "Ambiguous column name '{Column}'."
    );

    private static readonly MsSqlErrorDefinition IncorrectSyntaxNearToken =
        new MsSqlErrorDefinition(102, 15, 1, "Incorrect syntax near '{Token}'.");

    private static readonly MsSqlErrorDefinition NotARecognizedFunction = new MsSqlErrorDefinition(
        195,
        15,
        10,
        "'{Function}' is not a recognized built-in function name."
    );

    private static readonly MsSqlErrorDefinition ColumnCountMismatch = new MsSqlErrorDefinition(
        213,
        16,
        1,
        "Column name or number of supplied values does not match table definition."
    );

    private static readonly FrozenDictionary<SqliteErrorType, MsSqlErrorDefinition> Definitions =
        new Dictionary<SqliteErrorType, MsSqlErrorDefinition>
        {
            [SqliteErrorType.NoSuchTable] = InvalidObjectName,
            [SqliteErrorType.NoTablesSpecified] = new MsSqlErrorDefinition(
                1038,
                15,
                1,
                "Must specify table to select from."
            ),
            [SqliteErrorType.NoSuchColumn] = InvalidColumnName,
            [SqliteErrorType.TableHasNoColumnNamed] = InvalidColumnName,
            [SqliteErrorType.AmbiguousColumnName] = AmbiguousColumnName,
            [SqliteErrorType.NearSyntaxError] = new MsSqlErrorDefinition(
                102,
                15,
                1,
                "Incorrect syntax near '{Near}'."
            ),
            [SqliteErrorType.UnrecognizedToken] = IncorrectSyntaxNearToken,
            [SqliteErrorType.IncompleteInput] = new MsSqlErrorDefinition(
                102,
                15,
                1,
                "Incorrect syntax near the end of the statement."
            ),
            [SqliteErrorType.NoSuchFunction] = NotARecognizedFunction,
            [SqliteErrorType.UnknownFunction] = NotARecognizedFunction,
            [SqliteErrorType.ValuesForColumns] = ColumnCountMismatch,
            [SqliteErrorType.TableHasValuesForColumns] = ColumnCountMismatch,
            [SqliteErrorType.TableHasColumnsButValuesWereSupplied] = ColumnCountMismatch,
            [SqliteErrorType.AlreadyExists] = new MsSqlErrorDefinition(
                2714,
                16,
                6,
                "There is already an object named '{Name}' in the database."
            ),
            [SqliteErrorType.TableAlreadyNamed] = new MsSqlErrorDefinition(
                2714,
                16,
                6,
                "There is already an object named '{Table}' in the database."
            ),
            [SqliteErrorType.TriggerAlreadyExists] = new MsSqlErrorDefinition(
                2714,
                16,
                6,
                "There is already an object named '{Trigger}' in the database."
            ),
            [SqliteErrorType.NoSuchIndex] = new MsSqlErrorDefinition(
                3701,
                11,
                5,
                "Cannot drop the index '{Index}', because it does not exist or you do not have permission."
            ),
            [SqliteErrorType.UniqueConstraintFailed] = new MsSqlErrorDefinition(
                2627,
                14,
                1,
                "Violation of UNIQUE KEY constraint '{Constraint}'. Cannot insert duplicate key in object 'dbo.{Table}'."
            ),
            [SqliteErrorType.PrimaryKeyConstraintFailed] = new MsSqlErrorDefinition(
                2627,
                14,
                1,
                "Violation of PRIMARY KEY constraint '{Constraint}'. Cannot insert duplicate key in object 'dbo.{Table}'."
            ),
            [SqliteErrorType.NotNullConstraintFailed] = new MsSqlErrorDefinition(
                515,
                16,
                2,
                "Cannot insert the value NULL into column '{Column}', table 'dbo.{Table}'; column does not allow nulls. INSERT fails.",
                "Cannot insert the value NULL into column '{Column}', table 'dbo.{Table}'; column does not allow nulls. {Statement} fails."
            ),
            [SqliteErrorType.ForeignKeyConstraintFailed] = new MsSqlErrorDefinition(
                547,
                16,
                0,
                "The statement conflicted with the FOREIGN KEY constraint."
            ),
            [SqliteErrorType.CheckConstraintFailed] = new MsSqlErrorDefinition(
                547,
                16,
                0,
                "The statement conflicted with the CHECK constraint \"{Constraint}\".",
                "The {Statement} statement conflicted with the CHECK constraint \"{Constraint}\". The conflict occurred in database \"{Database}\", table \"dbo.{ContextTable}\"."
            ),
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<
        MsSqlRaisedErrorKind,
        MsSqlErrorDefinition
    > RaisedDefinitions = new Dictionary<MsSqlRaisedErrorKind, MsSqlErrorDefinition>
    {
        [MsSqlRaisedErrorKind.ConversionFailed] = new MsSqlErrorDefinition(
            245,
            16,
            1,
            "Conversion failed when converting the {SourceType} value '{Value}' to data type {TargetType}."
        ),
        [MsSqlRaisedErrorKind.ConversionOverflow] = new MsSqlErrorDefinition(
            248,
            16,
            1,
            "The conversion of the {SourceType} value '{Value}' overflowed {ColumnType} column."
        ),
        [MsSqlRaisedErrorKind.DateConversionFailed] = new MsSqlErrorDefinition(
            241,
            16,
            1,
            "Conversion failed when converting date and/or time from character string."
        ),
        [MsSqlRaisedErrorKind.ErrorConvertingDataType] = new MsSqlErrorDefinition(
            8114,
            16,
            5,
            "Error converting data type {SourceType} to {TargetType}."
        ),
        [MsSqlRaisedErrorKind.ArithmeticOverflow] = new MsSqlErrorDefinition(
            8115,
            16,
            2,
            "Arithmetic overflow error converting expression to data type {TargetType}."
        ),
        [MsSqlRaisedErrorKind.NumericOverflow] = new MsSqlErrorDefinition(
            8115,
            16,
            8,
            "Arithmetic overflow error converting numeric to data type numeric."
        ),
        [MsSqlRaisedErrorKind.DivideByZero] = new MsSqlErrorDefinition(
            8134,
            16,
            1,
            "Divide by zero error encountered."
        ),
        [MsSqlRaisedErrorKind.UnsupportedCulture] = new MsSqlErrorDefinition(
            9818,
            16,
            1,
            "The culture parameter '{Culture}' provided in the function call is not supported."
        ),
    }.ToFrozenDictionary();

    public static IReadOnlyCollection<SqliteErrorType> MappedTypes => Definitions.Keys;

    public static IReadOnlyCollection<MsSqlRaisedErrorKind> RaisedKinds => RaisedDefinitions.Keys;

    public static MsSqlErrorDefinition GetRaised(MsSqlRaisedErrorKind kind) =>
        RaisedDefinitions[kind];

    public static bool TryGetDefinition(
        SqliteErrorType type,
        [NotNullWhen(true)] out MsSqlErrorDefinition? definition
    ) => Definitions.TryGetValue(type, out definition);

    public static ErrorCoverage GetCoverage()
    {
        SqliteErrorType[] translatable =
        [
            .. Enum.GetValues<SqliteErrorType>()
                .Where((SqliteErrorType type) => type != SqliteErrorType.Unknown),
        ];
        SqliteErrorType[] unmapped =
        [
            .. translatable.Where((SqliteErrorType type) => !Definitions.ContainsKey(type)),
        ];

        return new ErrorCoverage(
            translatable.Length - unmapped.Length,
            translatable.Length,
            unmapped
        );
    }

    public static MsSqlError CreateFallback(string originalMessage, int? lineNumber = null) =>
        new MsSqlError(
            FallbackNumber,
            FallbackSeverity,
            FallbackState,
            FormatFallbackMessage(originalMessage),
            lineNumber
        );

    private static string FormatFallbackMessage(string originalMessage)
    {
        string text = originalMessage?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            return FallbackMessage;
        }

        string capitalized = char.ToUpperInvariant(text[0]) + text[1..];
        return capitalized[^1] is '.' or '!' or '?' ? capitalized : capitalized + ".";
    }
}
