namespace LearnSql.Core.Services.Exercises;

public sealed class ExerciseProgressBook
{
    public const int MaxStoredSqlLength = 4000;

    private readonly Dictionary<string, ExerciseProgress> _entries;

    public ExerciseProgressBook()
    {
        _entries = new Dictionary<string, ExerciseProgress>(StringComparer.Ordinal);
    }

    private ExerciseProgressBook(Dictionary<string, ExerciseProgress> entries)
    {
        _entries = entries;
    }

    public bool IsEmpty => _entries.Count == 0;

    public static ExerciseProgressBook FromDocument(
        ExerciseProgressDocument? document,
        IReadOnlyCollection<ExerciseDefinition> exercises
    )
    {
        ArgumentNullException.ThrowIfNull(exercises);

        Dictionary<string, ExerciseProgress> entries = new Dictionary<string, ExerciseProgress>(
            StringComparer.Ordinal
        );
        if (document?.Exercises is null)
        {
            return new ExerciseProgressBook(entries);
        }

        foreach (ExerciseDefinition exercise in exercises)
        {
            if (
                document.Exercises.TryGetValue(exercise.Id, out ExerciseProgress? stored)
                && stored is not null
            )
            {
                ExerciseProgress clean = Sanitize(stored, exercise);
                if (!clean.IsEmpty)
                {
                    entries[exercise.Id] = clean;
                }
            }
        }

        return new ExerciseProgressBook(entries);
    }

    public ExerciseProgressDocument ToDocument()
    {
        return new ExerciseProgressDocument(
            new Dictionary<string, ExerciseProgress>(_entries, StringComparer.Ordinal)
        );
    }

    public ExerciseProgress Get(string exerciseId)
    {
        return _entries.GetValueOrDefault(exerciseId) ?? ExerciseProgress.None;
    }

    public ExerciseStatus StatusOf(string exerciseId)
    {
        return Get(exerciseId).Status;
    }

    public void RecordAttempt(string exerciseId, bool solved, string? sql, DateTimeOffset now)
    {
        ExerciseProgress current = Get(exerciseId);
        _entries[exerciseId] = current with
        {
            Attempts = current.Attempts + 1,
            SolvedAt = current.SolvedAt ?? (solved ? now : null),
            LastSql = Limit(sql),
        };
    }

    public bool RevealHint(string exerciseId, int hintCount)
    {
        ExerciseProgress current = Get(exerciseId);
        if (current.HintsRevealed >= hintCount)
        {
            return false;
        }

        _entries[exerciseId] = current with { HintsRevealed = current.HintsRevealed + 1 };
        return true;
    }

    public bool MarkSolutionViewed(string exerciseId)
    {
        ExerciseProgress current = Get(exerciseId);
        if (current.SolutionViewed)
        {
            return false;
        }

        _entries[exerciseId] = current with { SolutionViewed = true };
        return true;
    }

    public void Clear()
    {
        _entries.Clear();
    }

    public int SolvedCount(IEnumerable<ExerciseDefinition> exercises)
    {
        return exercises.Count(
            (ExerciseDefinition exercise) => StatusOf(exercise.Id) == ExerciseStatus.Solved
        );
    }

    private static ExerciseProgress Sanitize(ExerciseProgress stored, ExerciseDefinition exercise)
    {
        return new ExerciseProgress(
            Math.Max(0, stored.Attempts),
            Math.Clamp(stored.HintsRevealed, 0, exercise.Hints.Count),
            stored.SolutionViewed,
            stored.SolvedAt,
            Limit(stored.LastSql)
        );
    }

    private static string? Limit(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return null;
        }

        return sql.Length <= MaxStoredSqlLength ? sql : null;
    }
}
