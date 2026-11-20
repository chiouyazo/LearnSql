namespace LearnSql.Core.Services.Exercises;

public sealed record ExercisePath(ExerciseTopic Topic, IReadOnlyList<ExerciseDefinition> Exercises);
