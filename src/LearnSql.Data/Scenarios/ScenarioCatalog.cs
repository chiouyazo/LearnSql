using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace LearnSql.Data.Scenarios;

public sealed class ScenarioCatalog
{
    private const string ResourcePrefix = "LearnSql.Data.";
    private const string ManifestSuffix = ".scenario.json";
    private const string ManifestFileName = "scenario.json";
    private const string TranslationMarker = ".scenario.";
    private const string JsonSuffix = ".json";

    public const string BaseLanguage = "de";

    private readonly Assembly _assembly;
    private readonly HashSet<string> _resourceNames;
    private readonly Dictionary<string, ScenarioDefinition> _byId;
    private readonly Dictionary<string, Dictionary<string, ScenarioTexts>> _translations;

    private ScenarioCatalog(
        Assembly assembly,
        IReadOnlyList<ScenarioDefinition> scenarios,
        Dictionary<string, Dictionary<string, ScenarioTexts>> translations
    )
    {
        _translations = translations;
        _assembly = assembly;
        _resourceNames = new HashSet<string>(
            assembly.GetManifestResourceNames(),
            StringComparer.Ordinal
        );
        Scenarios = scenarios;
        _byId = new Dictionary<string, ScenarioDefinition>(StringComparer.OrdinalIgnoreCase);
        foreach (ScenarioDefinition scenario in scenarios)
            _byId[scenario.Id] = scenario;
    }

    public IReadOnlyList<ScenarioDefinition> Scenarios { get; }

    public static ScenarioCatalog LoadDefault()
    {
        return Load(typeof(DataHolder).Assembly);
    }

    public static ScenarioCatalog Load(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        string[] manifestNames =
        [
            .. assembly
                .GetManifestResourceNames()
                .Where(name => name.EndsWith(ManifestSuffix, StringComparison.Ordinal))
                .Order(StringComparer.Ordinal),
        ];

        string[] resourceNames = assembly.GetManifestResourceNames();
        RejectOrphanTranslations(resourceNames);

        List<ScenarioDefinition> scenarios = [];
        Dictionary<string, Dictionary<string, ScenarioTexts>> translations = new Dictionary<
            string,
            Dictionary<string, ScenarioTexts>
        >(StringComparer.OrdinalIgnoreCase);
        HashSet<string> seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string manifestName in manifestNames)
        {
            ScenarioDefinition scenario = ReadManifest(assembly, manifestName);
            if (!seenIds.Add(scenario.Id))
            {
                throw new ScenarioCatalogException(
                    ScenarioCatalogErrorKind.DuplicateScenarioId,
                    $"The scenario id '{scenario.Id}' is defined more than once (manifest {manifestName}).",
                    scenario.Id,
                    manifestName
                );
            }
            translations[scenario.Id] = ReadTranslations(
                assembly,
                resourceNames,
                manifestName,
                scenario
            );
            scenarios.Add(Normalize(scenario));
        }

        scenarios.Sort(CompareScenarios);
        ScenarioCatalog catalog = new ScenarioCatalog(assembly, scenarios, translations);
        foreach (ScenarioDefinition scenario in catalog.Scenarios)
            catalog.ValidateResources(scenario);
        return catalog;
    }

    public bool TryGetScenario(string id, out ScenarioDefinition? scenario)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            scenario = null;
            return false;
        }
        return _byId.TryGetValue(id, out scenario);
    }

    public ScenarioDefinition GetScenario(string id)
    {
        if (TryGetScenario(id, out ScenarioDefinition? scenario) && scenario is not null)
            return scenario;

        string known = string.Join(", ", Scenarios.Select(s => s.Id));
        throw new ScenarioCatalogException(
            ScenarioCatalogErrorKind.UnknownScenario,
            $"Unknown scenario id '{id}'. Known scenarios: {known}.",
            id
        );
    }

    public IReadOnlyCollection<string> GetTranslationLanguages(string scenarioId)
    {
        return
            !string.IsNullOrWhiteSpace(scenarioId)
            && _translations.TryGetValue(
                scenarioId,
                out Dictionary<string, ScenarioTexts>? byLanguage
            )
            ? [.. byLanguage.Keys.Order(StringComparer.Ordinal)]
            : [];
    }

    public bool TryGetTranslation(string scenarioId, string language, out ScenarioTexts? texts)
    {
        texts = null;
        return !string.IsNullOrWhiteSpace(scenarioId)
            && !string.IsNullOrWhiteSpace(language)
            && _translations.TryGetValue(
                scenarioId,
                out Dictionary<string, ScenarioTexts>? byLanguage
            )
            && byLanguage.TryGetValue(language, out texts);
    }

    public bool HasTexts(string scenarioId, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        string language = culture.TwoLetterISOLanguageName;
        return language == BaseLanguage || TryGetTranslation(scenarioId, language, out _);
    }

    public ScenarioDefinition Localize(ScenarioDefinition scenario, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(culture);

        return TryGetTranslation(
            scenario.Id,
            culture.TwoLetterISOLanguageName,
            out ScenarioTexts? texts
        )
            ? ScenarioTextsMerger.Apply(scenario, texts!)
            : scenario;
    }

    public ScenarioScripts GetScripts(string id)
    {
        ScenarioDefinition scenario = GetScenario(id);
        return new ScenarioScripts(
            scenario.Id,
            ReadAll(scenario, scenario.SchemaResources),
            ReadAll(scenario, scenario.ViewResources),
            ReadAll(scenario, scenario.SeedResources)
        );
    }

    public static string ToResourceName(string logicalPath)
    {
        return ResourcePrefix + logicalPath.Replace('/', '.').Replace('\\', '.');
    }

    private static ScenarioDefinition ReadManifest(Assembly assembly, string manifestName)
    {
        try
        {
            using Stream stream =
                assembly.GetManifestResourceStream(manifestName)
                ?? throw new ScenarioCatalogException(
                    ScenarioCatalogErrorKind.MissingResource,
                    $"The manifest resource '{manifestName}' could not be opened.",
                    resource: manifestName
                );
            ScenarioDefinition? scenario = JsonSerializer.Deserialize(
                stream,
                ScenarioJsonContext.Default.ScenarioDefinition
            );
            if (scenario is null)
            {
                throw new ScenarioCatalogException(
                    ScenarioCatalogErrorKind.InvalidManifest,
                    $"The manifest '{manifestName}' is empty.",
                    resource: manifestName
                );
            }
            ScenarioDefinition complete = scenario with
            {
                Exercises = scenario.Exercises ?? [],
                GuidedScenarios = scenario.GuidedScenarios ?? [],
            };
            ValidateStructure(complete, manifestName);
            return complete;
        }
        catch (JsonException exception)
        {
            throw new ScenarioCatalogException(
                ScenarioCatalogErrorKind.InvalidManifest,
                $"The manifest '{manifestName}' is not valid: {exception.Message}",
                resource: manifestName,
                innerException: exception
            );
        }
        catch (IOException exception)
        {
            throw new ScenarioCatalogException(
                ScenarioCatalogErrorKind.UnreadableResource,
                $"The manifest '{manifestName}' could not be read: {exception.Message}",
                resource: manifestName,
                innerException: exception
            );
        }
    }

    private static void RejectOrphanTranslations(string[] resourceNames)
    {
        HashSet<string> names = new HashSet<string>(resourceNames, StringComparer.Ordinal);
        foreach (string name in resourceNames.Order(StringComparer.Ordinal))
        {
            if (
                TryParseTranslationName(name, out string prefix, out _)
                && !names.Contains(prefix + ManifestFileName)
            )
            {
                throw new ScenarioCatalogException(
                    ScenarioCatalogErrorKind.OrphanTranslation,
                    $"The translation '{name}' has no base manifest '{prefix}{ManifestFileName}'.",
                    resource: name
                );
            }
        }
    }

    private static bool TryParseTranslationName(
        string resourceName,
        out string prefix,
        out string language
    )
    {
        prefix = string.Empty;
        language = string.Empty;
        if (!resourceName.EndsWith(JsonSuffix, StringComparison.Ordinal))
        {
            return false;
        }

        int marker = resourceName.LastIndexOf(TranslationMarker, StringComparison.Ordinal);
        if (marker < 0)
        {
            return false;
        }

        int start = marker + TranslationMarker.Length;
        int length = resourceName.Length - JsonSuffix.Length - start;
        if (length is < 2 or > 3)
        {
            return false;
        }

        string candidate = resourceName.Substring(start, length);
        if (!candidate.All((char c) => c is >= 'a' and <= 'z'))
        {
            return false;
        }

        prefix = resourceName.Substring(0, marker + 1);
        language = candidate;
        return true;
    }

    private static Dictionary<string, ScenarioTexts> ReadTranslations(
        Assembly assembly,
        string[] resourceNames,
        string manifestName,
        ScenarioDefinition scenario
    )
    {
        string prefix = manifestName.Substring(0, manifestName.Length - ManifestFileName.Length);
        Dictionary<string, ScenarioTexts> byLanguage = new Dictionary<string, ScenarioTexts>(
            StringComparer.Ordinal
        );
        foreach (string name in resourceNames.Order(StringComparer.Ordinal))
        {
            if (
                !TryParseTranslationName(name, out string namePrefix, out string language)
                || namePrefix != prefix
            )
            {
                continue;
            }

            if (language == BaseLanguage)
            {
                throw new ScenarioCatalogException(
                    ScenarioCatalogErrorKind.InvalidTranslation,
                    $"The translation '{name}' must not exist: '{BaseLanguage}' is the language of the base manifest.",
                    scenario.Id,
                    name
                );
            }

            byLanguage[language] = ReadTranslation(assembly, name, scenario);
        }

        return byLanguage;
    }

    private static ScenarioTexts ReadTranslation(
        Assembly assembly,
        string resourceName,
        ScenarioDefinition scenario
    )
    {
        try
        {
            using Stream stream =
                assembly.GetManifestResourceStream(resourceName)
                ?? throw new ScenarioCatalogException(
                    ScenarioCatalogErrorKind.MissingResource,
                    $"The translation resource '{resourceName}' could not be opened.",
                    scenario.Id,
                    resourceName
                );
            ScenarioTexts? texts = JsonSerializer.Deserialize(
                stream,
                ScenarioJsonContext.Default.ScenarioTexts
            );
            if (texts is null)
            {
                throw new ScenarioCatalogException(
                    ScenarioCatalogErrorKind.InvalidTranslation,
                    $"The translation '{resourceName}' is empty.",
                    scenario.Id,
                    resourceName
                );
            }

            return ScenarioTextsValidator.Validate(scenario, texts, resourceName);
        }
        catch (JsonException exception)
        {
            throw new ScenarioCatalogException(
                ScenarioCatalogErrorKind.InvalidTranslation,
                $"The translation '{resourceName}' is not valid: {exception.Message}",
                scenario.Id,
                resourceName,
                exception
            );
        }
        catch (IOException exception)
        {
            throw new ScenarioCatalogException(
                ScenarioCatalogErrorKind.UnreadableResource,
                $"The translation '{resourceName}' could not be read: {exception.Message}",
                scenario.Id,
                resourceName,
                exception
            );
        }
    }

    private static void ValidateStructure(ScenarioDefinition scenario, string manifestName)
    {
        Require(!string.IsNullOrWhiteSpace(scenario.Id), manifestName, "Id must not be empty.");
        Require(
            !string.IsNullOrWhiteSpace(scenario.Title),
            manifestName,
            $"Scenario '{scenario.Id}' needs a title."
        );
        Require(
            scenario.SchemaResources.Count > 0,
            manifestName,
            $"Scenario '{scenario.Id}' needs at least one schema resource."
        );

        HashSet<string> exerciseIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (ExerciseDefinition exercise in scenario.Exercises)
        {
            Require(
                !string.IsNullOrWhiteSpace(exercise.Id) && exerciseIds.Add(exercise.Id),
                manifestName,
                $"Exercise id '{exercise.Id}' in scenario '{scenario.Id}' is empty or duplicated."
            );
            Require(
                !string.IsNullOrWhiteSpace(exercise.SolutionSql) && exercise.Hints.Count > 0,
                manifestName,
                $"Exercise '{exercise.Id}' needs a solution and at least one hint."
            );
        }

        HashSet<string> guidedIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (GuidedScenarioDefinition guided in scenario.GuidedScenarios)
        {
            Require(
                !string.IsNullOrWhiteSpace(guided.Id) && guidedIds.Add(guided.Id),
                manifestName,
                $"Guided scenario id '{guided.Id}' in scenario '{scenario.Id}' is empty or duplicated."
            );
            Require(
                guided.Steps.Count > 0,
                manifestName,
                $"Guided scenario '{guided.Id}' needs at least one step."
            );
            HashSet<string> stepIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (GuidedStep step in guided.Steps)
            {
                Require(
                    !string.IsNullOrWhiteSpace(step.Id)
                        && stepIds.Add(step.Id)
                        && !string.IsNullOrWhiteSpace(step.Sql),
                    manifestName,
                    $"Step '{step.Id}' in guided scenario '{guided.Id}' is incomplete or duplicated."
                );
            }
        }
    }

    private static void Require(bool condition, string manifestName, string message)
    {
        if (!condition)
        {
            throw new ScenarioCatalogException(
                ScenarioCatalogErrorKind.InvalidManifest,
                $"The manifest '{manifestName}' is not valid: {message}",
                resource: manifestName
            );
        }
    }

    private static ScenarioDefinition Normalize(ScenarioDefinition scenario)
    {
        return scenario with
        {
            Exercises =
            [
                .. scenario
                    .Exercises.OrderBy(e => e.Topic)
                    .ThenBy(e => e.Difficulty)
                    .ThenBy(e => e.Id, NaturalStringComparer.Instance),
            ],
            GuidedScenarios =
            [
                .. scenario
                    .GuidedScenarios.OrderBy(g => g.Topic)
                    .ThenBy(g => g.Id, NaturalStringComparer.Instance),
            ],
        };
    }

    private static int CompareScenarios(ScenarioDefinition left, ScenarioDefinition right)
    {
        int bySortOrder = left.SortOrder.CompareTo(right.SortOrder);
        return bySortOrder != 0
            ? bySortOrder
            : NaturalStringComparer.Instance.Compare(left.Id, right.Id);
    }

    private void ValidateResources(ScenarioDefinition scenario)
    {
        foreach (
            string logicalPath in scenario
                .SchemaResources.Concat(scenario.ViewResources)
                .Concat(scenario.SeedResources)
        )
        {
            string resourceName = ToResourceName(logicalPath);
            if (!_resourceNames.Contains(resourceName))
            {
                throw new ScenarioCatalogException(
                    ScenarioCatalogErrorKind.MissingResource,
                    $"Scenario '{scenario.Id}' references '{logicalPath}', but the embedded resource '{resourceName}' does not exist.",
                    scenario.Id,
                    logicalPath
                );
            }
        }
    }

    private List<SqlScript> ReadAll(ScenarioDefinition scenario, IReadOnlyList<string> logicalPaths)
    {
        List<SqlScript> scripts = [];
        foreach (string logicalPath in logicalPaths)
        {
            string resourceName = ToResourceName(logicalPath);
            try
            {
                using Stream stream =
                    _assembly.GetManifestResourceStream(resourceName)
                    ?? throw new ScenarioCatalogException(
                        ScenarioCatalogErrorKind.MissingResource,
                        $"Scenario '{scenario.Id}' references '{logicalPath}', but the embedded resource '{resourceName}' does not exist.",
                        scenario.Id,
                        logicalPath
                    );
                using StreamReader reader = new StreamReader(stream);
                scripts.Add(new SqlScript(logicalPath, reader.ReadToEnd()));
            }
            catch (IOException exception)
            {
                throw new ScenarioCatalogException(
                    ScenarioCatalogErrorKind.UnreadableResource,
                    $"The resource '{resourceName}' of scenario '{scenario.Id}' could not be read: {exception.Message}",
                    scenario.Id,
                    logicalPath,
                    exception
                );
            }
        }
        return scripts;
    }
}
