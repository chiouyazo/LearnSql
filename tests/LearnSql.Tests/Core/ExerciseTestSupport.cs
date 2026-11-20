using System.Text.Json;
using LearnSql.Core.Services.Storage;
using LearnSql.Data.Scenarios;
using LearnSql.Sql.Dialect.Typing;
using LearnSql.Sql.Execution;
using SqlResultSet = LearnSql.Sql.Execution.ResultSet;

namespace LearnSql.Tests.Core;

internal sealed class ExerciseMemoryStorage : IBrowserStorage
{
    public Dictionary<string, string> Raw { get; } = new Dictionary<string, string>();

    public ValueTask<T?> GetAsync<T>(
        StorageKey<T> key,
        CancellationToken cancellationToken = default
    )
    {
        return ValueTask.FromResult(
            Raw.TryGetValue(key.Name, out string? json)
                ? JsonSerializer.Deserialize(json, key.TypeInfo)
                : default
        );
    }

    public ValueTask<bool> SetAsync<T>(
        StorageKey<T> key,
        T value,
        CancellationToken cancellationToken = default
    )
    {
        Raw[key.Name] = JsonSerializer.Serialize(value, key.TypeInfo);
        return ValueTask.FromResult(true);
    }

    public ValueTask<bool> RemoveAsync<T>(
        StorageKey<T> key,
        CancellationToken cancellationToken = default
    )
    {
        return ValueTask.FromResult(Raw.Remove(key.Name));
    }
}

internal static class ExerciseTestData
{
    public static SqlResultSet Table(string[] columns, params object?[][] rows)
    {
        return new SqlResultSet(
            [.. columns.Select((string name) => new ResultColumn(name, null, ValueKind.Unknown))],
            rows,
            false
        );
    }

    public static ExerciseDefinition Exercise(
        string id,
        ExerciseTopic topic = ExerciseTopic.Select,
        ExerciseDifficulty difficulty = ExerciseDifficulty.Beginner,
        string solutionSql = "SELECT 1",
        bool orderMatters = false,
        int hints = 3
    )
    {
        return new ExerciseDefinition
        {
            Id = id,
            Title = "Title " + id,
            Description = "Description " + id,
            Topic = topic,
            Difficulty = difficulty,
            Hints = [.. Enumerable.Range(1, hints).Select((int index) => "Hint " + index)],
            SolutionSql = solutionSql,
            OrderMatters = orderMatters,
            ExpectedRowCount = 1,
        };
    }
}
