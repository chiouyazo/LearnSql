namespace LearnSql.Data.Scenarios;

public enum ScenarioCatalogErrorKind
{
    UnknownScenario = 0,
    InvalidManifest = 1,
    DuplicateScenarioId = 2,
    MissingResource = 3,
    UnreadableResource = 4,
    InvalidTranslation = 5,
    TranslationMismatch = 6,
    OrphanTranslation = 7,
}
