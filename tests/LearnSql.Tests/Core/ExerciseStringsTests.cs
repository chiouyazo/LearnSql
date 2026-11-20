using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using LearnSql.Core.Resources;
using LearnSql.Core.Services.Exercises;
using LearnSql.Data.Scenarios;

namespace LearnSql.Tests.Core;

public sealed partial class ExerciseStringsTests
{
    private static readonly ResourceManager Manager = new ResourceManager(typeof(ExerciseStrings));

    private static readonly Regex DashPattern = new Regex(
        "[" + (char)0x2013 + (char)0x2014 + "]|\\s-\\s|--"
    );

    [GeneratedRegex("\\{(\\d+)\\}")]
    private static partial Regex PlaceholderPattern();

    private static Dictionary<string, string> Load(CultureInfo culture)
    {
        ResourceSet set = Manager.GetResourceSet(culture, true, true)!;
        Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (DictionaryEntry entry in set)
        {
            values[(string)entry.Key] = (string)entry.Value!;
        }

        return values;
    }

    private static Dictionary<string, string> German => Load(CultureInfo.InvariantCulture);

    private static Dictionary<string, string> English => Load(new CultureInfo("en"));

    [Fact]
    public void GermanAndEnglish_DefineTheSameKeys()
    {
        Assert.Equal(
            German.Keys.Order(StringComparer.Ordinal),
            English.Keys.Order(StringComparer.Ordinal)
        );
    }

    [Fact]
    public void Values_AreNeverEmptyAndFreeOfDashes()
    {
        foreach (Dictionary<string, string> language in new[] { German, English })
        {
            foreach (KeyValuePair<string, string> pair in language)
            {
                Assert.False(string.IsNullOrWhiteSpace(pair.Value), pair.Key);
                Assert.DoesNotMatch(DashPattern, pair.Value);
            }
        }
    }

    [Fact]
    public void Placeholders_MatchBetweenGermanAndEnglish()
    {
        Dictionary<string, string> german = German;
        Dictionary<string, string> english = English;

        foreach (string key in german.Keys)
        {
            Assert.Equal(Placeholders(german[key]), Placeholders(english[key]));
        }
    }

    [Fact]
    public void EveryEnumValueShownInTheUi_HasATranslation()
    {
        Dictionary<string, string> german = German;
        Dictionary<string, string> english = English;
        List<string> keys =
        [
            .. Enum.GetValues<ExerciseTopic>().Select((ExerciseTopic topic) => "Topic" + topic),
            .. Enum.GetValues<ExerciseDifficulty>()
                .Select((ExerciseDifficulty difficulty) => "Difficulty" + difficulty),
            .. Enum.GetValues<ExerciseStatus>()
                .Select((ExerciseStatus status) => "Status" + status),
            .. Enum.GetValues<ExerciseCheckOutcome>()
                .Where(
                    (ExerciseCheckOutcome outcome) =>
                        outcome
                            is ExerciseCheckOutcome.Cancelled
                                or ExerciseCheckOutcome.EmptyInput
                                or ExerciseCheckOutcome.NotReady
                                or ExerciseCheckOutcome.Busy
                )
                .Select((ExerciseCheckOutcome outcome) => "Feedback" + outcome),
        ];

        foreach (string key in keys)
        {
            Assert.Contains(key, german.Keys);
            Assert.Contains(key, english.Keys);
        }
    }

    private static string Placeholders(string value)
    {
        return string.Join(
            ",",
            PlaceholderPattern()
                .Matches(value)
                .Select((Match match) => match.Groups[1].Value)
                .Distinct()
                .Order(StringComparer.Ordinal)
        );
    }
}
