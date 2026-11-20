namespace LearnSql.Core.Services.Templates;

public interface IQueryTemplateFactory
{
    string CreateStarterQuery(DatabaseSchema schema);

    IReadOnlyList<QueryTemplate> Create(DatabaseSchema schema);
}
