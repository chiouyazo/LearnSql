using LearnSql.Sql.Dialect.Typing;
using Microsoft.Data.Sqlite;

namespace LearnSql.Sql.Execution;

internal sealed class ResultSetReader
{
    private const string InferredNullTypeName = "BLOB";

    private readonly ICooperativeYield _yield;
    private readonly int _rowsPerYieldCheck;

    public ResultSetReader(ICooperativeYield yield, int rowsPerYieldCheck)
    {
        _yield = yield;
        _rowsPerYieldCheck = rowsPerYieldCheck;
    }

    public async Task<ResultSet> ReadAsync(
        SqliteDataReader reader,
        int maxRows,
        ExecutionGuard guard,
        CancellationToken cancellationToken
    )
    {
        int columnCount = reader.FieldCount;
        List<object?[]> rows = [];
        bool truncated = false;
        string?[] typeNames = new string?[columnCount];
        bool typesRead = false;

        while (await reader.ReadAsync(cancellationToken))
        {
            if (rows.Count >= maxRows)
            {
                truncated = true;
                break;
            }

            if (!typesRead)
            {
                ReadTypeNames(reader, typeNames);
                typesRead = true;
            }

            object?[] row = new object?[columnCount];
            for (int index = 0; index < columnCount; index++)
            {
                row[index] = reader.IsDBNull(index) ? null : reader.GetValue(index);
            }

            rows.Add(row);
            if (rows.Count % _rowsPerYieldCheck == 0)
            {
                guard.Poll();
                await _yield.YieldIfDueAsync(cancellationToken);
            }
        }

        if (!typesRead)
        {
            ReadTypeNames(reader, typeNames);
        }

        List<ResultColumn> columns = [];
        for (int index = 0; index < columnCount; index++)
        {
            columns.Add(CreateColumn(reader.GetName(index), typeNames[index], rows, index));
        }

        return new ResultSet(columns, rows, truncated);
    }

    private static void ReadTypeNames(SqliteDataReader reader, string?[] typeNames)
    {
        for (int index = 0; index < typeNames.Length; index++)
        {
            typeNames[index] = TryGetTypeName(reader, index);
        }
    }

    private static string? TryGetTypeName(SqliteDataReader reader, int index)
    {
        try
        {
            string name = reader.GetDataTypeName(index);
            bool inferredFromNull =
                string.Equals(name, InferredNullTypeName, StringComparison.Ordinal)
                && reader.HasRows
                && reader.IsDBNull(index);
            return inferredFromNull || string.IsNullOrWhiteSpace(name) ? null : name;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static ResultColumn CreateColumn(
        string name,
        string? typeName,
        List<object?[]> rows,
        int index
    )
    {
        ValueKind kind = DeclaredTypeClassifier.Classify(typeName);
        if (kind == ValueKind.Unknown)
        {
            kind = InferKind(rows, index);
        }

        return new ResultColumn(name, string.IsNullOrWhiteSpace(typeName) ? null : typeName, kind);
    }

    private static ValueKind InferKind(List<object?[]> rows, int index)
    {
        foreach (object?[] row in rows)
        {
            ValueKind kind = row[index] switch
            {
                long or double => ValueKind.Numeric,
                string => ValueKind.Text,
                byte[] => ValueKind.Binary,
                _ => ValueKind.Unknown,
            };
            if (kind != ValueKind.Unknown)
            {
                return kind;
            }
        }

        return ValueKind.Unknown;
    }
}
