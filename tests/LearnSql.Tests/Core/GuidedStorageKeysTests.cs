using LearnSql.Core.Services.Guided;

namespace LearnSql.Tests.Core;

public sealed class GuidedStorageKeysTests
{
    [Fact]
    public void EditorBackup_IsScopedPerScenario()
    {
        string starter = GuidedStorageKeys.EditorBackup("starter").Name;
        string analytics = GuidedStorageKeys.EditorBackup("analytics").Name;

        Assert.NotEqual(starter, analytics);
        Assert.All(
            [starter, analytics],
            (string name) => Assert.StartsWith("learnsql:v1:guided:", name)
        );
    }
}
