namespace LearnSql.Core.Services.Guided;

public static class GuidedPacing
{
    public const int MinSeconds = 8;
    public const int MaxSeconds = 30;
    public const int CharactersPerSecond = 20;

    public static int ReadingSeconds(GuidedStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        int characters =
            step.QueryExplanation.Summary.Length
            + step.QueryExplanation.Parts.Sum(
                (SqlPartExplanation part) => part.Fragment.Length + part.Explanation.Length
            )
            + step.ResultExplanation.Length
            + step.DatabaseEffect.Length;
        int seconds = (int)Math.Ceiling(characters / (double)CharactersPerSecond);
        return Math.Clamp(seconds, MinSeconds, MaxSeconds);
    }
}
