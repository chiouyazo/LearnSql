using System.Text.Json.Serialization;

namespace LearnSql.Core.Services.Exercises;

public sealed record ExerciseProgress(
    int Attempts,
    int HintsRevealed,
    bool SolutionViewed,
    DateTimeOffset? SolvedAt,
    string? LastSql
)
{
    public static ExerciseProgress None { get; } = new ExerciseProgress(0, 0, false, null, null);

    [JsonIgnore]
    public ExerciseStatus Status =>
        SolvedAt is not null ? ExerciseStatus.Solved
        : Attempts > 0 ? ExerciseStatus.Attempted
        : ExerciseStatus.Open;

    [JsonIgnore]
    public bool IsEmpty =>
        Attempts == 0
        && HintsRevealed == 0
        && !SolutionViewed
        && SolvedAt is null
        && LastSql is null;
}
