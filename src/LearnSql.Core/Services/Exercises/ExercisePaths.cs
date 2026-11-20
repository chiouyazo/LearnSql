namespace LearnSql.Core.Services.Exercises;

public static class ExercisePaths
{
    private static readonly ExerciseTopic[] LearningOrder =
    [
        ExerciseTopic.Select,
        ExerciseTopic.Filtering,
        ExerciseTopic.Joins,
        ExerciseTopic.Aggregation,
        ExerciseTopic.Subqueries,
        ExerciseTopic.WindowFunctions,
        ExerciseTopic.DataChange,
    ];

    public static IReadOnlyList<ExerciseTopic> Order => LearningOrder;

    public static int Rank(ExerciseTopic topic)
    {
        int index = Array.IndexOf(LearningOrder, topic);
        return index >= 0 ? index : LearningOrder.Length;
    }

    public static IReadOnlyList<ExercisePath> Build(IEnumerable<ExerciseDefinition> exercises)
    {
        ArgumentNullException.ThrowIfNull(exercises);

        return
        [
            .. exercises
                .GroupBy((ExerciseDefinition exercise) => exercise.Topic)
                .OrderBy((IGrouping<ExerciseTopic, ExerciseDefinition> group) => Rank(group.Key))
                .ThenBy((IGrouping<ExerciseTopic, ExerciseDefinition> group) => group.Key)
                .Select(
                    (IGrouping<ExerciseTopic, ExerciseDefinition> group) =>
                        new ExercisePath(
                            group.Key,
                            [
                                .. group
                                    .OrderBy((ExerciseDefinition exercise) => exercise.Difficulty)
                                    .ThenBy(
                                        (ExerciseDefinition exercise) => exercise.Id,
                                        NaturalStringComparer.Instance
                                    ),
                            ]
                        )
                ),
        ];
    }

    public static IReadOnlyList<ExerciseDefinition> Flatten(IReadOnlyList<ExercisePath> paths)
    {
        return [.. paths.SelectMany((ExercisePath path) => path.Exercises)];
    }
}
