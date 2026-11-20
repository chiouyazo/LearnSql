using LearnSql.Core.Services.Storage;

namespace LearnSql.Core.Services.Exercises;

public sealed class ExerciseProgressService : IExerciseProgressService
{
    private readonly IBrowserStorage _storage;
    private readonly TimeProvider _time;
    private readonly Dictionary<string, Task<ExerciseProgressBook>> _books = new Dictionary<
        string,
        Task<ExerciseProgressBook>
    >(StringComparer.Ordinal);

    public ExerciseProgressService(IBrowserStorage storage, TimeProvider time)
    {
        _storage = storage;
        _time = time;
    }

    public event EventHandler? Changed;

    public Task<ExerciseProgressBook> GetBookAsync(ScenarioDefinition scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        if (!_books.TryGetValue(scenario.Id, out Task<ExerciseProgressBook>? book))
        {
            book = LoadAsync(scenario);
            _books[scenario.Id] = book;
        }

        return book;
    }

    public async Task RecordAttemptAsync(
        ScenarioDefinition scenario,
        string exerciseId,
        bool solved,
        string? sql
    )
    {
        ExerciseProgressBook book = await GetBookAsync(scenario);
        book.RecordAttempt(exerciseId, solved, sql, _time.GetUtcNow());
        await PersistAsync(scenario, book);
    }

    public async Task<int> RevealNextHintAsync(
        ScenarioDefinition scenario,
        ExerciseDefinition exercise
    )
    {
        ExerciseProgressBook book = await GetBookAsync(scenario);
        if (book.RevealHint(exercise.Id, exercise.Hints.Count))
        {
            await PersistAsync(scenario, book);
        }

        return book.Get(exercise.Id).HintsRevealed;
    }

    public async Task MarkSolutionViewedAsync(ScenarioDefinition scenario, string exerciseId)
    {
        ExerciseProgressBook book = await GetBookAsync(scenario);
        if (book.MarkSolutionViewed(exerciseId))
        {
            await PersistAsync(scenario, book);
        }
    }

    public async Task ResetAsync(ScenarioDefinition scenario)
    {
        ExerciseProgressBook book = await GetBookAsync(scenario);
        book.Clear();
        await _storage.RemoveAsync(StorageKeys.Exercises(scenario.Id));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private async Task<ExerciseProgressBook> LoadAsync(ScenarioDefinition scenario)
    {
        ExerciseProgressDocument? document = await _storage.GetAsync(
            StorageKeys.Exercises(scenario.Id)
        );
        return ExerciseProgressBook.FromDocument(document, scenario.Exercises.ToArray());
    }

    private async Task PersistAsync(ScenarioDefinition scenario, ExerciseProgressBook book)
    {
        await _storage.SetAsync(StorageKeys.Exercises(scenario.Id), book.ToDocument());
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
