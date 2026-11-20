using System.Text;
using LearnSql.SqlFormat;

bool check = args.Contains("--check", StringComparer.Ordinal);
string[] paths = [.. args.Where((string a) => a != "--check")];
string root = paths.Length > 0 ? paths[0] : Path.Combine("src", "LearnSql.Data", "Data");

if (!Directory.Exists(root))
{
    Console.Error.WriteLine($"Directory not found: {root}");
    return 2;
}

UTF8Encoding utf8 = new UTF8Encoding(false);
string[] manifests =
[
    .. Directory
        .EnumerateFiles(root, "scenario.json", SearchOption.AllDirectories)
        .Order(StringComparer.Ordinal),
];
int changed = 0;
foreach (string manifest in manifests)
{
    string original = File.ReadAllText(manifest, utf8);
    string rewritten = ScenarioManifestFormatter.Rewrite(original);
    if (rewritten == original)
        continue;

    changed++;
    if (!check)
        File.WriteAllText(manifest, rewritten, utf8);
    Console.WriteLine($"{(check ? "Needs formatting" : "Formatted")}: {manifest}");
}

Console.WriteLine(
    $"{manifests.Length} manifests checked, {changed} {(check ? "need" : "had")} changes."
);
return check && changed > 0 ? 1 : 0;
