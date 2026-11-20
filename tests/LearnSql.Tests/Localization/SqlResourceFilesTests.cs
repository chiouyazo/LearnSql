using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using LearnSql.Sql.Dialect.Hints;
using LearnSql.Sql.Dialect.Unsupported;
using LearnSql.Sql.Execution;

namespace LearnSql.Tests.Localization;

public sealed partial class SqlResourceFilesTests
{
    private const string Namespace = "LearnSql.Sql.Resources.";

    private static readonly Assembly SqlAssembly = typeof(SandboxDatabase).Assembly;

    private static readonly CultureInfo English = new CultureInfo("en");

    private static readonly Regex DashPattern = new Regex(
        "[" + (char)0x2013 + (char)0x2014 + "]|\\s-\\s|--"
    );

    private static readonly Regex GermanWordPattern = new Regex(
        "\\b(und|der|die|das|nicht|wird|werden|ist|ein|eine|mit|f\u00fcr|oder|kann|zum|zur|dem|den|von|auf|Zeile|Zeilen|Anweisung|Spalte|Tabelle)\\b",
        RegexOptions.IgnoreCase
    );

    private static readonly Regex UmlautPattern = new Regex(
        "[\u00e4\u00f6\u00fc\u00c4\u00d6\u00dc\u00df]"
    );

    private static readonly HashSet<string> KeysWithUmlautsInEnglish = ["Collation_Point3"];

    private static readonly IReadOnlyList<UnsupportedConstruct> Catalog =
        UnsupportedConstructs.CreateDefault();

    public static TheoryData<string> Families =>
        new TheoryData<string>
        {
            "ExecutionStrings",
            "DialectStrings",
            "UnsupportedStrings",
            "HintStrings",
        };

    [GeneratedRegex("\\{(\\d+)(?::[^}]*)?\\}")]
    private static partial Regex PlaceholderPattern();

    [GeneratedRegex("^(?<id>\\w+?)_(Name|Alternative)$")]
    private static partial Regex UnsupportedKeyPattern();

    [GeneratedRegex(
        "^(?<id>\\w+?)_(Title|Summary|Point(?<point>\\d+)|Example(?<example>\\d+)_(MsSql|Sandbox))$"
    )]
    private static partial Regex HintKeyPattern();

    private static Dictionary<string, string> Load(string family, CultureInfo culture)
    {
        ResourceManager manager = new ResourceManager(Namespace + family, SqlAssembly);
        ResourceSet? set = manager.GetResourceSet(
            culture,
            true,
            culture.Equals(CultureInfo.InvariantCulture)
        );
        Assert.NotNull(set);
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

    private static IEnumerable<string> AccessorMembers(string family)
    {
        Type type = SqlAssembly.GetType(Namespace + family, true)!;
        return type.GetMembers(
                BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly
            )
            .Where(
                (MemberInfo member) =>
                    member.MemberType == MemberTypes.Property
                    || (member.MemberType == MemberTypes.Method && !member.Name.StartsWith("get_"))
            )
            .Select((MemberInfo member) => member.Name);
    }

    [Fact]
    public void Assembly_EmbedsExactlyTheKnownResourceFamilies()
    {
        string[] embedded = SqlAssembly
            .GetManifestResourceNames()
            .Where((string name) => name.StartsWith(Namespace, StringComparison.Ordinal))
            .Select((string name) => name[Namespace.Length..^".resources".Length])
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(Families.Select((object[] row) => (string)row[0]).Order(), embedded);
    }

    [Fact]
    public void EnglishSatelliteAssembly_IsPartOfTheBuild()
    {
        string location = Path.Combine(
            Path.GetDirectoryName(SqlAssembly.Location)!,
            "en",
            "LearnSql.Sql.resources.dll"
        );

        Assert.True(File.Exists(location), location);
    }

    [Theory]
    [MemberData(nameof(Families))]
    public void GermanAndEnglish_DefineTheSameKeysAndPlaceholders(string family)
    {
        Dictionary<string, string> german = Load(family, CultureInfo.InvariantCulture);
        Dictionary<string, string> english = Load(family, English);

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
    [MemberData(nameof(Families))]
    public void Values_AreNeverEmptyFreeOfDashesAndHaveNoStrayBraces(string family)
    {
        foreach (CultureInfo culture in new[] { CultureInfo.InvariantCulture, English })
        {
            foreach (KeyValuePair<string, string> pair in Load(family, culture))
            {
                string name = family + "." + pair.Key;
                Assert.False(string.IsNullOrWhiteSpace(pair.Value), name);
                Assert.DoesNotMatch(DashPattern, pair.Value);
                string withoutPlaceholders = PlaceholderPattern().Replace(pair.Value, string.Empty);
                Assert.True(withoutPlaceholders.IndexOfAny(['{', '}']) < 0, name);
            }
        }
    }

    [Theory]
    [MemberData(nameof(Families))]
    public void EnglishValues_ContainNoGermanWordsAndNoUmlauts(string family)
    {
        foreach (KeyValuePair<string, string> pair in Load(family, English))
        {
            string name = family + "." + pair.Key;
            Assert.False(GermanWordPattern.IsMatch(pair.Value), name + ": " + pair.Value);
            if (!KeysWithUmlautsInEnglish.Contains(pair.Key))
            {
                Assert.False(UmlautPattern.IsMatch(pair.Value), name + ": " + pair.Value);
            }
        }
    }

    [Theory]
    [MemberData(nameof(Families))]
    public void EnglishValues_DifferFromTheGermanOnesUnlessTheyAreTechnicalNames(string family)
    {
        Dictionary<string, string> german = Load(family, CultureInfo.InvariantCulture);
        Dictionary<string, string> english = Load(family, English);

        string[] identical = german
            .Keys.Where((string key) => german[key] == english[key])
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.All(identical, (string key) => Assert.Matches("^(\\w+)_Name$", key));
    }

    [Theory]
    [InlineData("ExecutionStrings")]
    [InlineData("DialectStrings")]
    public void Keys_OfFixedFamilies_AreExactlyTheMembersOfTheAccessor(string family)
    {
        Assert.Equal(
            AccessorMembers(family).Order(StringComparer.Ordinal),
            Load(family, CultureInfo.InvariantCulture).Keys.Order(StringComparer.Ordinal)
        );
    }

    [Fact]
    public void UnsupportedStrings_HaveANameAndAnAlternativeForEveryCatalogEntryAndNothingElse()
    {
        string[] expected = Catalog
            .SelectMany(
                (UnsupportedConstruct construct) =>
                    new[] { construct.Id + "_Name", construct.Id + "_Alternative" }
            )
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expected,
            Load("UnsupportedStrings", CultureInfo.InvariantCulture)
                .Keys.Order(StringComparer.Ordinal)
        );
        Assert.All(expected, (string key) => Assert.Matches(UnsupportedKeyPattern(), key));
    }

    [Fact]
    public void HintStrings_BelongToExistingTopicsWithConsecutivePointsAndUsedExamples()
    {
        IReadOnlyList<DialectHintTopic> topics = DialectDifferences.CreateDefault(Catalog);
        Dictionary<string, string> german = Load("HintStrings", CultureInfo.InvariantCulture);
        Dictionary<string, List<string>> keysByTopic = new Dictionary<string, List<string>>();

        foreach (string key in german.Keys)
        {
            Match match = HintKeyPattern().Match(key);
            Assert.True(match.Success, key);
            string id = match.Groups["id"].Value;
            Assert.Contains(id, topics.Select((DialectHintTopic topic) => topic.Id));
            if (!keysByTopic.TryGetValue(id, out List<string>? keys))
            {
                keys = [];
                keysByTopic[id] = keys;
            }

            keys.Add(key);
        }

        foreach (DialectHintTopic topic in topics)
        {
            Assert.Contains(topic.Id + "_Title", german.Keys);
            Assert.Contains(topic.Id + "_Summary", german.Keys);
            int points = keysByTopic[topic.Id]
                .Count((string key) => HintKeyPattern().Match(key).Groups["point"].Success);
            if (topic.Id == "NotYetAvailable")
            {
                Assert.Equal(0, points);
                continue;
            }

            Assert.Equal(topic.Points.Count, points);
            for (int number = 1; number <= points; number++)
            {
                Assert.Contains(topic.Id + "_Point" + number, german.Keys);
            }
        }

        foreach (string key in german.Keys.Where((string key) => key.Contains("_Example")))
        {
            Match match = HintKeyPattern().Match(key);
            DialectHintTopic topic = topics.Single(
                (DialectHintTopic candidate) => candidate.Id == match.Groups["id"].Value
            );
            DialectHintExample example = topic.Examples[
                int.Parse(match.Groups["example"].Value) - 1
            ];
            string used = key.EndsWith("_MsSql", StringComparison.Ordinal)
                ? example.MsSqlResult
                : example.SandboxResult;
            Assert.Equal(german[key], used);
        }
    }
}
