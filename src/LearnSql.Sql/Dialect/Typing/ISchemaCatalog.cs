namespace LearnSql.Sql.Dialect.Typing;

public interface ISchemaCatalog
{
    bool TryGetRelation(string name, out RelationShape? shape);
}
