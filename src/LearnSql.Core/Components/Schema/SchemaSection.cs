namespace LearnSql.Core.Components.Schema;

public sealed record SchemaSection(string TitleKey, IReadOnlyList<RelationSchema> Relations);
