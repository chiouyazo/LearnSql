using LearnSql.Core.Services.Storage;

namespace LearnSql.Core.Services.Guided;

public static class GuidedStorageKeys
{
    private const string Prefix = "learnsql:v1:guided:";

    public static StorageKey<string> EditorBackup(string scenarioId)
    {
        return new StorageKey<string>(
            Prefix + "backup:" + scenarioId,
            StorageJsonContext.Default.String
        );
    }
}
