using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using LearnSql.Core.Resources;

namespace LearnSql.Tests.Core;

public sealed partial class ResourceStringsTests
{
    private static readonly Regex DashPattern = new Regex(
        "[" + (char)0x2013 + (char)0x2014 + "]|\\s-\\s|--"
    );

    public static TheoryData<Type> ResourceTypes =>
        new TheoryData<Type>
        {
            typeof(AppStrings),
            typeof(EditorStrings),
            typeof(ExerciseStrings),
            typeof(GuidedStrings),
            typeof(HomeStrings),
            typeof(MudStrings),
            typeof(ResultStrings),
            typeof(ScenarioStrings),
            typeof(SchemaStrings),
            typeof(WorkspaceStrings),
        };

    [GeneratedRegex("\\{(\\d+)\\}")]
    private static partial Regex PlaceholderPattern();

    private static Dictionary<string, string> Load(Type type, CultureInfo culture)
    {
        ResourceSet set = new ResourceManager(type).GetResourceSet(culture, true, true)!;
        Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (DictionaryEntry entry in set)
        {
            values[(string)entry.Key] = (string)entry.Value!;
        }

        return values;
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

    [Theory]
    [MemberData(nameof(ResourceTypes))]
    public void GermanAndEnglish_DefineTheSameKeysAndPlaceholders(Type type)
    {
        Dictionary<string, string> german = Load(type, CultureInfo.InvariantCulture);
        Dictionary<string, string> english = Load(type, new CultureInfo("en"));

        Assert.Equal(
            german.Keys.Order(StringComparer.Ordinal),
            english.Keys.Order(StringComparer.Ordinal)
        );
        foreach (string key in german.Keys)
        {
            Assert.Equal(Placeholders(german[key]), Placeholders(english[key]));
        }
    }

    [Theory]
    [MemberData(nameof(ResourceTypes))]
    public void Values_AreNeverEmptyAndFreeOfDashes(Type type)
    {
        foreach (
            CultureInfo culture in new[] { CultureInfo.InvariantCulture, new CultureInfo("en") }
        )
        {
            foreach (KeyValuePair<string, string> pair in Load(type, culture))
            {
                Assert.False(string.IsNullOrWhiteSpace(pair.Value), type.Name + "." + pair.Key);
                Assert.DoesNotMatch(DashPattern, pair.Value);
            }
        }
    }

    [Fact]
    public void GuidedStrings_HaveATopicLabelForEveryGuidedTopic()
    {
        Dictionary<string, string> german = Load(
            typeof(GuidedStrings),
            CultureInfo.InvariantCulture
        );

        foreach (string topic in Enum.GetNames<LearnSql.Data.Scenarios.ExerciseTopic>())
        {
            Assert.Contains("Topic_" + topic, german.Keys);
        }
    }
}
