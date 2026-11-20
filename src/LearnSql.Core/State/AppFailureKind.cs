namespace LearnSql.Core.State;

public enum AppFailureKind
{
    EngineInitialization,
    UnknownScenario,
    InvalidScenario,
    ScriptFailed,
    Cancelled,
    Internal,
}
