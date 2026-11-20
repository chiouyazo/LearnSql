using System.Text.Json.Serialization;

namespace LearnSql.Data.Scenarios;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    AllowTrailingCommas = false,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
)]
[JsonSerializable(typeof(ScenarioDefinition))]
[JsonSerializable(typeof(ScenarioTexts))]
internal sealed partial class ScenarioJsonContext : JsonSerializerContext;
