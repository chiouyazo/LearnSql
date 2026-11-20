using LearnSql.Core.Services.Guided;
using LearnSql.Data.Scenarios;

namespace LearnSql.Tests.Core;

public sealed class GuidedPacingTests
{
    private static GuidedStep Step(int characters)
    {
        return new GuidedStep
        {
            Id = "s",
            Title = "s",
            Sql = "SELECT 1;",
            Kind = GuidedStepKind.ReadOnly,
            QueryExplanation = new QueryExplanation
            {
                Summary = new string('a', characters),
                Parts = [],
            },
            ResultExplanation = string.Empty,
            DatabaseEffect = string.Empty,
        };
    }

    [Fact]
    public void ReadingSeconds_ShortText_UsesTheMinimum()
    {
        Assert.Equal(GuidedPacing.MinSeconds, GuidedPacing.ReadingSeconds(Step(10)));
    }

    [Fact]
    public void ReadingSeconds_LongText_UsesTheMaximum()
    {
        Assert.Equal(GuidedPacing.MaxSeconds, GuidedPacing.ReadingSeconds(Step(5000)));
    }

    [Fact]
    public void ReadingSeconds_MediumText_GrowsWithTheLength()
    {
        int shorter = GuidedPacing.ReadingSeconds(Step(200));
        int longer = GuidedPacing.ReadingSeconds(Step(320));

        Assert.True(shorter < longer);
        Assert.Equal(200 / GuidedPacing.CharactersPerSecond, shorter);
    }

    [Fact]
    public void ReadingSeconds_CountsPartsResultAndEffect()
    {
        GuidedStep step = Step(0) with
        {
            QueryExplanation = new QueryExplanation
            {
                Summary = string.Empty,
                Parts =
                [
                    new SqlPartExplanation { Fragment = "F", Explanation = new string('b', 159) },
                ],
            },
            ResultExplanation = new string('c', 80),
            DatabaseEffect = new string('d', 80),
        };

        Assert.Equal(16, GuidedPacing.ReadingSeconds(step));
    }
}
