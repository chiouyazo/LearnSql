using System.Text.Json;
using LearnSql.Core.Services.Exercises;
using LearnSql.Core.Services.Storage;
using LearnSql.Data.Scenarios;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Core;

public sealed class ExerciseProgressTests
{
    private static readonly DateTimeOffset Now = new DateTimeOffset(
        2026,
        3,
        14,
        9,
        26,
        53,
        TimeSpan.Zero
    );

    private static ExerciseDefinition[] Exercises()
    {
        return
        [
            ExerciseTestData.Exercise("e1", hints: 3),
            ExerciseTestData.Exercise("e2", hints: 4),
            ExerciseTestData.Exercise("e3", hints: 2),
        ];
    }

    private static ScenarioDefinition Scenario(string id = "starter")
    {
        ScenarioDefinition template = ScenarioData.Catalog.GetScenario("starter");
        return template with { Id = id, Exercises = Exercises() };
    }

    [Fact]
    public void Status_IsDerivedFromAttemptsAndSolution()
    {
        Assert.Equal(ExerciseStatus.Open, ExerciseProgress.None.Status);
        Assert.Equal(
            ExerciseStatus.Open,
            (ExerciseProgress.None with { HintsRevealed = 2, SolutionViewed = true }).Status
        );
        Assert.Equal(
            ExerciseStatus.Attempted,
            (ExerciseProgress.None with { Attempts = 1 }).Status
        );
        Assert.Equal(
            ExerciseStatus.Solved,
            (ExerciseProgress.None with { Attempts = 1, SolvedAt = Now }).Status
        );
    }

    [Fact]
    public void NewBook_IsEmptyAndReportsOpenExercises()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();

        Assert.True(book.IsEmpty);
        Assert.Equal(ExerciseStatus.Open, book.StatusOf("e1"));
        Assert.Equal(0, book.SolvedCount(Exercises()));
    }

    [Fact]
    public void RecordAttempt_WrongAnswer_CountsAttemptAndKeepsExerciseUnsolved()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();

        book.RecordAttempt("e1", false, "SELECT 1", Now);

        ExerciseProgress progress = book.Get("e1");
        Assert.Equal(1, progress.Attempts);
        Assert.Null(progress.SolvedAt);
        Assert.Equal("SELECT 1", progress.LastSql);
        Assert.Equal(ExerciseStatus.Attempted, progress.Status);
        Assert.False(book.IsEmpty);
    }

    [Fact]
    public void RecordAttempt_CorrectAnswer_MarksSolvedAndKeepsTheFirstSolveTime()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();

        book.RecordAttempt("e1", false, "a", Now);
        book.RecordAttempt("e1", true, "b", Now.AddMinutes(5));
        book.RecordAttempt("e1", true, "c", Now.AddMinutes(9));
        book.RecordAttempt("e1", false, "d", Now.AddMinutes(12));

        ExerciseProgress progress = book.Get("e1");
        Assert.Equal(4, progress.Attempts);
        Assert.Equal(Now.AddMinutes(5), progress.SolvedAt);
        Assert.Equal(ExerciseStatus.Solved, progress.Status);
        Assert.Equal("d", progress.LastSql);
    }

    [Fact]
    public void RecordAttempt_TooLongSql_IsNotStored()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();

        book.RecordAttempt(
            "e1",
            false,
            new string('x', ExerciseProgressBook.MaxStoredSqlLength + 1),
            Now
        );

        Assert.Null(book.Get("e1").LastSql);
        Assert.Equal(1, book.Get("e1").Attempts);
    }

    [Fact]
    public void RecordAttempt_SqlAtTheLimit_IsStored()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();
        string sql = new string('x', ExerciseProgressBook.MaxStoredSqlLength);

        book.RecordAttempt("e1", false, sql, Now);

        Assert.Equal(sql, book.Get("e1").LastSql);
    }

    [Fact]
    public void RevealHint_AdvancesOneLevelAtATimeAndStopsAtTheCount()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();

        Assert.True(book.RevealHint("e3", 2));
        Assert.True(book.RevealHint("e3", 2));
        Assert.False(book.RevealHint("e3", 2));

        Assert.Equal(2, book.Get("e3").HintsRevealed);
        Assert.Equal(ExerciseStatus.Open, book.StatusOf("e3"));
    }

    [Fact]
    public void MarkSolutionViewed_IsIdempotent()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();

        Assert.True(book.MarkSolutionViewed("e1"));
        Assert.False(book.MarkSolutionViewed("e1"));

        Assert.True(book.Get("e1").SolutionViewed);
    }

    [Fact]
    public void SolvedAfterViewingTheSolution_StaysSolvedAndRemembersTheView()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();

        book.MarkSolutionViewed("e1");
        book.RecordAttempt("e1", true, "SELECT 1", Now);

        Assert.Equal(ExerciseStatus.Solved, book.StatusOf("e1"));
        Assert.True(book.Get("e1").SolutionViewed);
    }

    [Fact]
    public void SolvedCount_CountsOnlySolvedExercisesOfTheGivenSet()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();
        ExerciseDefinition[] exercises = Exercises();
        book.RecordAttempt("e1", true, null, Now);
        book.RecordAttempt("e2", false, null, Now);
        book.RecordAttempt("e3", true, null, Now);

        Assert.Equal(2, book.SolvedCount(exercises));
        Assert.Equal(1, book.SolvedCount(exercises.Take(2)));
        Assert.Equal(0, book.SolvedCount([]));
    }

    [Fact]
    public void Clear_RemovesAllProgress()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();
        book.RecordAttempt("e1", true, "x", Now);
        book.RevealHint("e1", 3);

        book.Clear();

        Assert.True(book.IsEmpty);
        Assert.Equal(ExerciseStatus.Open, book.StatusOf("e1"));
    }

    [Fact]
    public void Document_RoundTripsThroughTheGeneratedJsonContext()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();
        book.RecordAttempt("e1", false, "SELECT 'a\"b'\nFROM T", Now);
        book.RecordAttempt("e1", true, "SELECT 1", Now.AddMinutes(1));
        book.RevealHint("e2", 4);
        book.RevealHint("e2", 4);
        book.MarkSolutionViewed("e2");
        StorageKey<ExerciseProgressDocument> key = StorageKeys.Exercises("starter");

        string json = JsonSerializer.Serialize(book.ToDocument(), key.TypeInfo);
        ExerciseProgressDocument? restored = JsonSerializer.Deserialize(json, key.TypeInfo);
        ExerciseProgressBook reloaded = ExerciseProgressBook.FromDocument(restored, Exercises());

        Assert.Equal(book.Get("e1"), reloaded.Get("e1"));
        Assert.Equal(book.Get("e2"), reloaded.Get("e2"));
        Assert.Equal(ExerciseProgress.None, reloaded.Get("e3"));
        Assert.Equal(Now.AddMinutes(1), reloaded.Get("e1").SolvedAt);
    }

    [Fact]
    public void Document_UsesReadablePropertyNames()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();
        book.RecordAttempt("e1", true, "SELECT 1", Now);

        string json = JsonSerializer.Serialize(
            book.ToDocument(),
            StorageKeys.Exercises("starter").TypeInfo
        );

        Assert.Contains("\"Exercises\"", json);
        Assert.Contains("\"e1\"", json);
        Assert.Contains("\"Attempts\":1", json);
        Assert.Contains("\"SolvedAt\":\"2026-03-14T09:26:53+00:00\"", json);
        Assert.DoesNotContain("\"Status\"", json);
        Assert.DoesNotContain("\"IsEmpty\"", json);
    }

    [Fact]
    public void StorageKey_IsVersionedAndScopedByScenario()
    {
        Assert.Equal("learnsql:v1:exercises:starter", StorageKeys.Exercises("starter").Name);
        Assert.Equal("learnsql:v1:exercises:analytics", StorageKeys.Exercises("analytics").Name);
    }

    [Fact]
    public void FromDocument_WithoutDocumentOrDictionary_ReturnsAnEmptyBook()
    {
        Assert.True(ExerciseProgressBook.FromDocument(null, Exercises()).IsEmpty);
        ExerciseProgressDocument? withoutDictionary = JsonSerializer.Deserialize(
            "{}",
            StorageKeys.Exercises("starter").TypeInfo
        );
        Assert.True(ExerciseProgressBook.FromDocument(withoutDictionary, Exercises()).IsEmpty);
    }

    [Fact]
    public void FromDocument_IgnoresUnknownExercisesAndEmptyEntries()
    {
        ExerciseProgressDocument document = new ExerciseProgressDocument(
            new Dictionary<string, ExerciseProgress>
            {
                ["gone"] = new ExerciseProgress(3, 1, false, null, null),
                ["e1"] = ExerciseProgress.None,
                ["e2"] = new ExerciseProgress(2, 0, false, null, null),
            }
        );

        ExerciseProgressBook book = ExerciseProgressBook.FromDocument(document, Exercises());

        Assert.Equal(2, book.Get("e2").Attempts);
        Assert.Equal(ExerciseProgress.None, book.Get("gone"));
        Assert.Equal(ExerciseProgress.None, book.Get("e1"));
        Assert.DoesNotContain("gone", book.ToDocument().Exercises.Keys);
        Assert.DoesNotContain("e1", book.ToDocument().Exercises.Keys);
    }

    [Fact]
    public void FromDocument_SanitizesImplausibleValues()
    {
        ExerciseProgressDocument document = new ExerciseProgressDocument(
            new Dictionary<string, ExerciseProgress>
            {
                ["e1"] = new ExerciseProgress(-4, 99, true, null, new string('y', 5000)),
                ["e3"] = new ExerciseProgress(1, -2, false, null, "  "),
            }
        );

        ExerciseProgressBook book = ExerciseProgressBook.FromDocument(document, Exercises());

        ExerciseProgress first = book.Get("e1");
        Assert.Equal(0, first.Attempts);
        Assert.Equal(3, first.HintsRevealed);
        Assert.True(first.SolutionViewed);
        Assert.Null(first.LastSql);
        ExerciseProgress third = book.Get("e3");
        Assert.Equal(0, third.HintsRevealed);
        Assert.Null(third.LastSql);
    }

    [Fact]
    public void ToDocument_ReturnsACopyThatLaterChangesDoNotAffect()
    {
        ExerciseProgressBook book = new ExerciseProgressBook();
        book.RecordAttempt("e1", false, null, Now);
        ExerciseProgressDocument snapshot = book.ToDocument();

        book.RecordAttempt("e2", false, null, Now);

        Assert.Single(snapshot.Exercises);
    }

    [Fact]
    public async Task Service_PersistsAttemptsHintsAndSolutionViewsAcrossInstances()
    {
        ExerciseMemoryStorage storage = new ExerciseMemoryStorage();
        ScenarioDefinition scenario = Scenario();
        ExerciseDefinition second = scenario.Exercises[1];
        ExerciseProgressService first = new ExerciseProgressService(
            storage,
            new FixedTimeProvider(Now)
        );

        await first.RecordAttemptAsync(scenario, "e1", true, "SELECT 1");
        await first.RecordAttemptAsync(scenario, "e2", false, "SELECT 2");
        await first.RevealNextHintAsync(scenario, second);
        await first.RevealNextHintAsync(scenario, second);
        await first.MarkSolutionViewedAsync(scenario, "e2");
        ExerciseProgressService reloaded = new ExerciseProgressService(
            storage,
            new FixedTimeProvider(Now.AddDays(1))
        );
        ExerciseProgressBook book = await reloaded.GetBookAsync(scenario);

        Assert.Equal(ExerciseStatus.Solved, book.StatusOf("e1"));
        Assert.Equal(Now, book.Get("e1").SolvedAt);
        ExerciseProgress progress = book.Get("e2");
        Assert.Equal(1, progress.Attempts);
        Assert.Equal(2, progress.HintsRevealed);
        Assert.True(progress.SolutionViewed);
        Assert.Equal("SELECT 2", progress.LastSql);
        Assert.Contains("learnsql:v1:exercises:starter", storage.Raw.Keys);
    }

    [Fact]
    public async Task Service_RevealNextHint_ReturnsTheLevelAndStopsAtTheLastHint()
    {
        ScenarioDefinition scenario = Scenario();
        ExerciseDefinition exercise = scenario.Exercises[2];
        ExerciseProgressService service = new ExerciseProgressService(
            new ExerciseMemoryStorage(),
            TimeProvider.System
        );

        Assert.Equal(1, await service.RevealNextHintAsync(scenario, exercise));
        Assert.Equal(2, await service.RevealNextHintAsync(scenario, exercise));
        Assert.Equal(2, await service.RevealNextHintAsync(scenario, exercise));
    }

    [Fact]
    public async Task Service_KeepsScenariosSeparate()
    {
        ExerciseMemoryStorage storage = new ExerciseMemoryStorage();
        ExerciseProgressService service = new ExerciseProgressService(storage, TimeProvider.System);
        ScenarioDefinition starter = Scenario("starter");
        ScenarioDefinition analytics = Scenario("analytics");

        await service.RecordAttemptAsync(starter, "e1", true, null);

        Assert.Equal(ExerciseStatus.Solved, (await service.GetBookAsync(starter)).StatusOf("e1"));
        Assert.Equal(ExerciseStatus.Open, (await service.GetBookAsync(analytics)).StatusOf("e1"));
    }

    [Fact]
    public async Task Service_Reset_RemovesStoredProgressAndRaisesChanged()
    {
        ExerciseMemoryStorage storage = new ExerciseMemoryStorage();
        ExerciseProgressService service = new ExerciseProgressService(storage, TimeProvider.System);
        ScenarioDefinition scenario = Scenario();
        await service.RecordAttemptAsync(scenario, "e1", true, "x");
        int changed = 0;
        service.Changed += (object? sender, EventArgs e) => changed++;

        await service.ResetAsync(scenario);

        Assert.Equal(1, changed);
        Assert.DoesNotContain("learnsql:v1:exercises:starter", storage.Raw.Keys);
        Assert.True((await service.GetBookAsync(scenario)).IsEmpty);
        ExerciseProgressService reloaded = new ExerciseProgressService(
            storage,
            TimeProvider.System
        );
        Assert.True((await reloaded.GetBookAsync(scenario)).IsEmpty);
    }

    [Fact]
    public async Task Service_RaisesChangedForEveryPersistedChange()
    {
        ExerciseProgressService service = new ExerciseProgressService(
            new ExerciseMemoryStorage(),
            TimeProvider.System
        );
        ScenarioDefinition scenario = Scenario();
        int changed = 0;
        service.Changed += (object? sender, EventArgs e) => changed++;

        await service.RecordAttemptAsync(scenario, "e1", false, null);
        await service.RevealNextHintAsync(scenario, scenario.Exercises[0]);
        await service.MarkSolutionViewedAsync(scenario, "e1");
        await service.MarkSolutionViewedAsync(scenario, "e1");

        Assert.Equal(3, changed);
    }

    [Fact]
    public async Task Service_DropsStoredEntriesOfExercisesThatNoLongerExist()
    {
        ExerciseMemoryStorage storage = new ExerciseMemoryStorage();
        ExerciseProgressService old = new ExerciseProgressService(storage, TimeProvider.System);
        ScenarioDefinition scenario = Scenario();
        await old.RecordAttemptAsync(scenario, "e1", true, null);
        ScenarioDefinition reduced = scenario with { Exercises = [scenario.Exercises[1]] };
        ExerciseProgressService current = new ExerciseProgressService(storage, TimeProvider.System);

        ExerciseProgressBook book = await current.GetBookAsync(reduced);

        Assert.Equal(ExerciseStatus.Open, book.StatusOf("e1"));
        Assert.Equal(0, book.SolvedCount(reduced.Exercises));
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;

        public FixedTimeProvider(DateTimeOffset now)
        {
            _now = now;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _now;
        }
    }
}
