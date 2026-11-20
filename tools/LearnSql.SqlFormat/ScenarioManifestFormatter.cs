using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LearnSql.SqlFormat;

public static class ScenarioManifestFormatter
{
    private static readonly HashSet<string> SqlProperties = new HashSet<string>(
        StringComparer.Ordinal
    )
    {
        "sql",
        "solutionSql",
    };

    public static string Rewrite(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        byte[] bytes = new UTF8Encoding(false).GetBytes(json);
        using MemoryStream output = new MemoryStream();
        Utf8JsonReader reader = new Utf8JsonReader(bytes);
        string? property = null;
        long copied = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                property = reader.GetString();
                continue;
            }

            bool isSql =
                reader.TokenType == JsonTokenType.String
                && property is not null
                && SqlProperties.Contains(property);
            property = null;
            if (!isSql)
                continue;

            long start = reader.TokenStartIndex;
            long rawLength = reader.HasValueSequence
                ? reader.ValueSequence.Length
                : reader.ValueSpan.Length;
            string formatted = SqlFormatter.Format(reader.GetString() ?? string.Empty);
            output.Write(bytes, (int)copied, (int)(start + 1 - copied));
            output.Write(
                JsonEncodedText
                    .Encode(formatted, JavaScriptEncoder.UnsafeRelaxedJsonEscaping)
                    .EncodedUtf8Bytes
            );
            copied = start + 1 + rawLength;
        }

        output.Write(bytes, (int)copied, bytes.Length - (int)copied);
        return new UTF8Encoding(false).GetString(output.ToArray());
    }
}
