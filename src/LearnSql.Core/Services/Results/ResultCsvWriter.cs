using System.Text;

namespace LearnSql.Core.Services.Results;

public static class ResultCsvWriter
{
    public static byte[] ToUtf8Bytes(ResultSet resultSet, IReadOnlyList<int>? rowOrder = null)
    {
        UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        byte[] preamble = encoding.GetPreamble();
        byte[] body = encoding.GetBytes(ToCsv(resultSet, rowOrder));
        return [.. preamble, .. body];
    }

    public static string ToCsv(ResultSet resultSet, IReadOnlyList<int>? rowOrder = null)
    {
        StringBuilder builder = new StringBuilder();
        AppendRecord(builder, resultSet.Columns.Select((ResultColumn column) => column.Name));

        IEnumerable<int> indices = rowOrder ?? Enumerable.Range(0, resultSet.Rows.Count);
        foreach (int index in indices)
        {
            AppendRecord(
                builder,
                resultSet
                    .Rows[index]
                    .Select(
                        (object? value) =>
                            value is null ? string.Empty : CellFormatter.Format(value)
                    )
            );
        }

        return builder.ToString();
    }

    private static void AppendRecord(StringBuilder builder, IEnumerable<string> fields)
    {
        builder.AppendJoin(',', fields.Select(Escape));
        builder.Append("\r\n");
    }

    private static string Escape(string field)
    {
        bool needsQuotes = field.AsSpan().IndexOfAny(",\"\r\n") >= 0;
        return needsQuotes
            ? "\"" + field.Replace("\"", "\"\"", StringComparison.Ordinal) + "\""
            : field;
    }
}
