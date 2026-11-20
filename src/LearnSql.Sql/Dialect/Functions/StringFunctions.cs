using System.Text;

namespace LearnSql.Sql.Dialect.Functions;

public sealed class StringFunctions
{
    private const string LeftLengthMessage =
        "Invalid length parameter passed to the LEFT or SUBSTRING function.";
    private const string RightLengthMessage =
        "Invalid length parameter passed to the RIGHT function.";
    private const string SubstringLengthMessage =
        "Invalid length parameter passed to the substring function.";

    public IEnumerable<SqliteScalarFunction> Create()
    {
        yield return new SqliteScalarFunction("LEN", 1, arguments => Len(arguments[0]));
        yield return new SqliteScalarFunction(
            "LEFT",
            2,
            arguments => Left(arguments[0], arguments[1])
        );
        yield return new SqliteScalarFunction(
            "RIGHT",
            2,
            arguments => Right(arguments[0], arguments[1])
        );
        yield return new SqliteScalarFunction(
            "CHARINDEX",
            2,
            arguments => CharIndex(arguments[0], arguments[1], null)
        );
        yield return new SqliteScalarFunction(
            "CHARINDEX",
            3,
            arguments => CharIndex(arguments[0], arguments[1], arguments[2])
        );
        yield return new SqliteScalarFunction(
            "REPLICATE",
            2,
            arguments => Replicate(arguments[0], arguments[1])
        );
        yield return new SqliteScalarFunction("SPACE", 1, arguments => Space(arguments[0]));
        yield return new SqliteScalarFunction(
            "STUFF",
            4,
            arguments => Stuff(arguments[0], arguments[1], arguments[2], arguments[3])
        );
        yield return new SqliteScalarFunction("REVERSE", 1, arguments => Reverse(arguments[0]));
        yield return new SqliteScalarFunction(
            "SUBSTRING",
            3,
            arguments => Substring(arguments[0], arguments[1], arguments[2])
        );
        yield return new SqliteScalarFunction(
            "REPLACE",
            3,
            arguments => Replace(arguments[0], arguments[1], arguments[2])
        );
        yield return new SqliteScalarFunction(
            "UPPER",
            1,
            arguments => MapText(arguments[0], text => text.ToUpperInvariant())
        );
        yield return new SqliteScalarFunction(
            "LOWER",
            1,
            arguments => MapText(arguments[0], text => text.ToLowerInvariant())
        );
        yield return new SqliteScalarFunction("ASCII", 1, arguments => Ascii(arguments[0]));
    }

    public static object? Len(object? value)
    {
        return SqlValue.IsNull(value) ? null : (long)SqlValue.ToText(value!).TrimEnd(' ').Length;
    }

    public static object? Left(object? value, object? count)
    {
        if (SqlValue.IsNull(value) || SqlValue.IsNull(count))
            return null;

        long length = SqlValue.ToLong(count!);
        if (length < 0)
            throw new ArgumentException(LeftLengthMessage);

        string text = SqlValue.ToText(value!);
        return length >= text.Length ? text : text.Substring(0, (int)length);
    }

    public static object? Right(object? value, object? count)
    {
        if (SqlValue.IsNull(value) || SqlValue.IsNull(count))
            return null;

        long length = SqlValue.ToLong(count!);
        if (length < 0)
            throw new ArgumentException(RightLengthMessage);

        string text = SqlValue.ToText(value!);
        return length >= text.Length ? text : text.Substring(text.Length - (int)length);
    }

    public static object? CharIndex(object? find, object? search, object? start)
    {
        if (SqlValue.IsNull(find) || SqlValue.IsNull(search))
            return null;

        string needle = SqlValue.ToText(find!);
        string haystack = SqlValue.ToText(search!);
        long startPosition = SqlValue.IsNull(start) ? 1 : Math.Max(1, SqlValue.ToLong(start!));

        if (needle.Length == 0 || startPosition > haystack.Length)
            return 0L;

        int index = haystack.IndexOf(
            needle,
            (int)startPosition - 1,
            StringComparison.OrdinalIgnoreCase
        );
        return index < 0 ? 0L : index + 1L;
    }

    public static object? Replicate(object? value, object? count)
    {
        if (SqlValue.IsNull(value) || SqlValue.IsNull(count))
            return null;

        long repetitions = SqlValue.ToLong(count!);
        if (repetitions < 0)
            return null;

        string text = SqlValue.ToText(value!);
        StringBuilder builder = new StringBuilder(
            checked((int)Math.Min(text.Length * repetitions, int.MaxValue))
        );
        for (long index = 0; index < repetitions; index++)
            builder.Append(text);
        return builder.ToString();
    }

    public static object? Space(object? count)
    {
        if (SqlValue.IsNull(count))
            return null;

        long length = SqlValue.ToLong(count!);
        return length < 0 ? null : new string(' ', checked((int)length));
    }

    public static object? Stuff(object? value, object? start, object? length, object? replacement)
    {
        if (SqlValue.IsNull(value) || SqlValue.IsNull(start) || SqlValue.IsNull(length))
            return null;

        string text = SqlValue.ToText(value!);
        long position = SqlValue.ToLong(start!);
        long removeCount = SqlValue.ToLong(length!);
        if (position < 1 || position > text.Length || removeCount < 0)
            return null;

        int zeroBased = (int)position - 1;
        int removable = (int)Math.Min(removeCount, text.Length - zeroBased);
        string inserted = SqlValue.IsNull(replacement)
            ? string.Empty
            : SqlValue.ToText(replacement!);
        return text.Substring(0, zeroBased) + inserted + text.Substring(zeroBased + removable);
    }

    public static object? Reverse(object? value)
    {
        if (SqlValue.IsNull(value))
            return null;

        char[] characters = SqlValue.ToText(value!).ToCharArray();
        Array.Reverse(characters);
        return new string(characters);
    }

    public static object? Substring(object? value, object? start, object? length)
    {
        if (SqlValue.IsNull(value) || SqlValue.IsNull(start) || SqlValue.IsNull(length))
            return null;

        long first = SqlValue.ToLong(start!);
        long count = SqlValue.ToLong(length!);
        if (count < 0)
            throw new ArgumentException(SubstringLengthMessage);

        string text = SqlValue.ToText(value!);
        long lastExclusive = Math.Min(first + count, text.Length + 1L);
        long firstClamped = Math.Max(first, 1L);
        if (lastExclusive <= firstClamped)
            return string.Empty;

        return text.Substring((int)firstClamped - 1, (int)(lastExclusive - firstClamped));
    }

    public static object? Replace(object? value, object? find, object? replacement)
    {
        if (SqlValue.IsNull(value) || SqlValue.IsNull(find) || SqlValue.IsNull(replacement))
            return null;

        string text = SqlValue.ToText(value!);
        string needle = SqlValue.ToText(find!);
        if (needle.Length == 0)
            return text;

        return text.Replace(
            needle,
            SqlValue.ToText(replacement!),
            StringComparison.OrdinalIgnoreCase
        );
    }

    public static object? Ascii(object? value)
    {
        if (SqlValue.IsNull(value))
            return null;

        string text = SqlValue.ToText(value!);
        return text.Length == 0 ? null : (long)text[0];
    }

    private static object? MapText(object? value, Func<string, string> map)
    {
        return SqlValue.IsNull(value) ? null : map(SqlValue.ToText(value!));
    }
}
