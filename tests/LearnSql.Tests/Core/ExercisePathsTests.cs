using LearnSql.Core.Services.Exercises;
using LearnSql.Data.Scenarios;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Core;

public sealed class ExercisePathsTests
{
    public static TheoryData<string> ScenarioIds() => ScenarioData.ScenarioIds();

    [Fact]
    public void Order_ListsEveryDefinedTopicExactlyOnce()
    {
        ExerciseTopic[] defined = Enum.GetValues<ExerciseTopic>();

        Assert.Equal(defined.Length, ExercisePaths.Order.Count);
        Assert.Equal(defined.Order(), ExercisePaths.Order.Order());
    }

    [Fact]
    public void Order_FollowsTheLearningSequence()
    {
        Assert.Equal(
            [
                ExerciseTopic.Select,
                ExerciseTopic.Filtering,
                ExerciseTopic.Joins,
                ExerciseTopic.Aggregation,
                ExerciseTopic.Subqueries,
                ExerciseTopic.WindowFunctions,
                ExerciseTopic.DataChange,
            ],
            ExercisePaths.Order
        );
    }

    [Fact]
    public void Rank_IsStrictlyIncreasingAlongTheOrder()
    {
        int[] ranks = [.. ExercisePaths.Order.Select(ExercisePaths.Rank)];

        Assert.Equal(Enumerable.Range(0, ranks.Length), ranks);
    }

    [Fact]
    public void Rank_UnknownTopic_SortsAfterAllKnownTopics()
    {
        int unknown = ExercisePaths.Rank((ExerciseTopic)99);

        Assert.All(
            ExercisePaths.Order,
            (ExerciseTopic topic) => Assert.True(ExercisePaths.Rank(topic) < unknown)
        );
    }

    [Fact]
    public void Build_GroupsByTopicInLearningOrderRegardlessOfInputOrder()
    {
        ExerciseDefinition[] exercises =
        [
            ExerciseTestData.Exercise("w1", ExerciseTopic.WindowFunctions),
            ExerciseTestData.Exercise("s1", ExerciseTopic.Subqueries),
            ExerciseTestData.Exercise("j1", ExerciseTopic.Joins),
            ExerciseTestData.Exercise("q1", ExerciseTopic.Select),
            ExerciseTestData.Exercise("a1", ExerciseTopic.Aggregation),
            ExerciseTestData.Exercise("f1", ExerciseTopic.Filtering),
        ];

        IReadOnlyList<ExercisePath> paths = ExercisePaths.Build(exercises.Reverse());

        Assert.Equal(
            [
                ExerciseTopic.Select,
                ExerciseTopic.Filtering,
                ExerciseTopic.Joins,
                ExerciseTopic.Aggregation,
                ExerciseTopic.Subqueries,
                ExerciseTopic.WindowFunctions,
            ],
            paths.Select((ExercisePath path) => path.Topic)
        );
    }

    [Fact]
    public void Build_OrdersWithinATopicByDifficultyThenNaturalId()
    {
        ExerciseDefinition[] exercises =
        [
            ExerciseTestData.Exercise("join-10", ExerciseTopic.Joins, ExerciseDifficulty.Beginner),
            ExerciseTestData.Exercise("join-2", ExerciseTopic.Joins, ExerciseDifficulty.Beginner),
            ExerciseTestData.Exercise("join-1", ExerciseTopic.Joins, ExerciseDifficulty.Advanced),
            ExerciseTestData.Exercise(
                "join-3",
                ExerciseTopic.Joins,
                ExerciseDifficulty.Intermediate
            ),
        ];

        ExercisePath path = Assert.Single(ExercisePaths.Build(exercises));

        Assert.Equal(
            ["join-2", "join-10", "join-3", "join-1"],
            path.Exercises.Select((ExerciseDefinition e) => e.Id)
        );
    }

    [Fact]
    public void Build_UnknownTopics_AreAppendedLastInDeterministicOrder()
    {
        ExerciseDefinition[] exercises =
        [
            ExerciseTestData.Exercise("x2", (ExerciseTopic)98),
            ExerciseTestData.Exercise("w1", ExerciseTopic.WindowFunctions),
            ExerciseTestData.Exercise("x1", (ExerciseTopic)42),
            ExerciseTestData.Exercise("q1", ExerciseTopic.Select),
        ];

        IReadOnlyList<ExercisePath> paths = ExercisePaths.Build(exercises);

        Assert.Equal(
            [
                ExerciseTopic.Select,
                ExerciseTopic.WindowFunctions,
                (ExerciseTopic)42,
                (ExerciseTopic)98,
            ],
            paths.Select((ExercisePath path) => path.Topic)
        );
    }

    [Fact]
    public void Build_OmitsTopicsWithoutExercises()
    {
        IReadOnlyList<ExercisePath> paths = ExercisePaths.Build([
            ExerciseTestData.Exercise("q1", ExerciseTopic.Select),
        ]);

        Assert.Single(paths);
    }

    [Fact]
    public void Build_WithoutExercises_ReturnsNoPaths()
    {
        Assert.Empty(ExercisePaths.Build([]));
    }

    [Fact]
    public void Build_IsStableAcrossRepeatedCalls()
    {
        ExerciseDefinition[] exercises =
        [
            ExerciseTestData.Exercise("b", ExerciseTopic.Joins),
            ExerciseTestData.Exercise("a", ExerciseTopic.Joins),
            ExerciseTestData.Exercise("c", ExerciseTopic.Select),
        ];

        Assert.Equal(
            ExercisePaths
                .Flatten(ExercisePaths.Build(exercises))
                .Select((ExerciseDefinition e) => e.Id),
            ExercisePaths
                .Flatten(ExercisePaths.Build(exercises.Reverse()))
                .Select((ExerciseDefinition e) => e.Id)
        );
    }

    [Fact]
    public void Flatten_ConcatenatesThePathsInOrder()
    {
        IReadOnlyList<ExercisePath> paths = ExercisePaths.Build([
            ExerciseTestData.Exercise("j1", ExerciseTopic.Joins),
            ExerciseTestData.Exercise("q1", ExerciseTopic.Select),
            ExerciseTestData.Exercise("q2", ExerciseTopic.Select),
        ]);

        Assert.Equal(
            ["q1", "q2", "j1"],
            ExercisePaths.Flatten(paths).Select((ExerciseDefinition e) => e.Id)
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Build_ForEveryScenario_CoversEachExerciseExactlyOnce(string scenarioId)
    {
        ScenarioDefinition scenario = ScenarioData.Catalog.GetScenario(scenarioId);

        IReadOnlyList<ExerciseDefinition> flat = ExercisePaths.Flatten(
            ExercisePaths.Build(scenario.Exercises)
        );

        Assert.Equal(
            scenario.Exercises.Select((ExerciseDefinition e) => e.Id).Order(StringComparer.Ordinal),
            flat.Select((ExerciseDefinition e) => e.Id).Order(StringComparer.Ordinal)
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Build_ForEveryScenario_StartsWithSelectAndNeverRegressesInTopicRank(
        string scenarioId
    )
    {
        ScenarioDefinition scenario = ScenarioData.Catalog.GetScenario(scenarioId);

        IReadOnlyList<ExercisePath> paths = ExercisePaths.Build(scenario.Exercises);

        Assert.Equal(ExerciseTopic.Select, paths[0].Topic);
        int[] ranks = [.. paths.Select((ExercisePath path) => ExercisePaths.Rank(path.Topic))];
        Assert.Equal(ranks.Order(), ranks);
        Assert.Equal(ranks.Distinct().Count(), ranks.Length);
    }
}
