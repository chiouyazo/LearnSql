using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Typing;

public sealed class RelationShapeResolver
{
    private readonly FragmentTree _tree;
    private readonly ISchemaCatalog _schema;
    private readonly ExpressionTypeInferrer _inferrer;
    private readonly Dictionary<TSqlFragment, SourceScope> _scopes = new Dictionary<
        TSqlFragment,
        SourceScope
    >(ReferenceEqualityComparer.Instance);
    private readonly HashSet<TSqlFragment> _inProgress = new HashSet<TSqlFragment>(
        ReferenceEqualityComparer.Instance
    );

    public RelationShapeResolver(
        FragmentTree tree,
        ISchemaCatalog schema,
        ExpressionTypeInferrer inferrer
    )
    {
        _tree = tree;
        _schema = schema;
        _inferrer = inferrer;
    }

    public ValueKind ResolveColumnKind(ColumnReferenceExpression column, FragmentNode node)
    {
        IList<Identifier> parts = column.MultiPartIdentifier.Identifiers;
        if (parts.Count == 0)
            return ValueKind.Unknown;

        string name = parts[parts.Count - 1].Value;
        string? qualifier = parts.Count >= 2 ? parts[parts.Count - 2].Value : null;

        foreach (FragmentNode ancestor in node.Ancestors())
        {
            SourceScope? scope = GetScope(ancestor);
            if (scope is not null && scope.TryResolveColumn(qualifier, name, out ColumnInfo? info))
                return info!.Kind;
        }
        return ValueKind.Unknown;
    }

    public RelationShape? ShapeOfQuery(QueryExpression query)
    {
        if (!_inProgress.Add(query))
            return null;

        try
        {
            switch (query)
            {
                case QuerySpecification specification:
                    return ShapeOfSpecification(specification);
                case BinaryQueryExpression binary:
                    return ShapeOfQuery(binary.FirstQueryExpression);
                case QueryParenthesisExpression parenthesis:
                    return ShapeOfQuery(parenthesis.QueryExpression);
                default:
                    return null;
            }
        }
        finally
        {
            _inProgress.Remove(query);
        }
    }

    private SourceScope? GetScope(FragmentNode owner)
    {
        TSqlFragment fragment = owner.Fragment;
        if (_scopes.TryGetValue(fragment, out SourceScope? cached))
            return cached;

        SourceScope? scope = fragment switch
        {
            QuerySpecification specification => BuildScope(owner, specification.FromClause, null),
            UpdateDeleteSpecificationBase modification => BuildScope(
                owner,
                modification.FromClause,
                modification.Target
            ),
            CreateTableStatement createTable => BuildCreateTableScope(createTable),
            _ => null,
        };

        if (scope is not null)
            _scopes[fragment] = scope;
        return scope;
    }

    private SourceScope BuildScope(
        FragmentNode owner,
        FromClause? fromClause,
        TableReference? target
    )
    {
        List<RelationSource> sources = [];
        if (target is not null)
            AddTableReference(owner, target, sources);
        if (fromClause is not null)
        {
            foreach (TableReference reference in fromClause.TableReferences)
                AddTableReference(owner, reference, sources);
        }
        return new SourceScope(sources);
    }

    private SourceScope BuildCreateTableScope(CreateTableStatement createTable)
    {
        List<ColumnInfo> columns = [];
        if (createTable.Definition is not null)
        {
            foreach (ColumnDefinition definition in createTable.Definition.ColumnDefinitions)
                columns.Add(
                    new ColumnInfo(
                        definition.ColumnIdentifier.Value,
                        TSqlDataTypes.GetKind(definition.DataType)
                    )
                );
        }
        string tableName = createTable.SchemaObjectName.BaseIdentifier.Value;
        return new SourceScope([new RelationSource(tableName, new RelationShape(columns))]);
    }

    private void AddTableReference(
        FragmentNode owner,
        TableReference reference,
        List<RelationSource> sources
    )
    {
        switch (reference)
        {
            case JoinTableReference join:
                AddTableReference(owner, join.FirstTableReference, sources);
                AddTableReference(owner, join.SecondTableReference, sources);
                break;
            case JoinParenthesisTableReference parenthesis:
                AddTableReference(owner, parenthesis.Join, sources);
                break;
            case NamedTableReference named:
                string tableName = named.SchemaObject.BaseIdentifier.Value;
                string sourceName = named.Alias?.Value ?? tableName;
                sources.Add(new RelationSource(sourceName, LookupNamedShape(owner, tableName)));
                break;
            case QueryDerivedTable derived:
                sources.Add(
                    new RelationSource(
                        derived.Alias?.Value ?? string.Empty,
                        ShapeOfQuery(derived.QueryExpression)
                    )
                );
                break;
            case TableReferenceWithAlias aliased:
                sources.Add(new RelationSource(aliased.Alias?.Value ?? string.Empty, null));
                break;
        }
    }

    private RelationShape? LookupNamedShape(FragmentNode owner, string tableName)
    {
        foreach (FragmentNode ancestor in owner.Ancestors().Prepend(owner))
        {
            if (ancestor.Fragment is not StatementWithCtesAndXmlNamespaces statement)
                continue;
            if (statement.WithCtesAndXmlNamespaces is null)
                continue;

            foreach (
                CommonTableExpression cte in statement
                    .WithCtesAndXmlNamespaces
                    .CommonTableExpressions
            )
            {
                if (
                    string.Equals(
                        cte.ExpressionName.Value,
                        tableName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                    return ApplyColumnList(ShapeOfQuery(cte.QueryExpression), cte.Columns);
            }
        }

        _schema.TryGetRelation(tableName, out RelationShape? shape);
        return shape;
    }

    private static RelationShape? ApplyColumnList(RelationShape? shape, IList<Identifier> names)
    {
        if (shape is null || names.Count == 0)
            return shape;

        List<ColumnInfo> renamed = [];
        for (int i = 0; i < shape.Columns.Count; i++)
        {
            string name = i < names.Count ? names[i].Value : shape.Columns[i].Name;
            renamed.Add(new ColumnInfo(name, shape.Columns[i].Kind));
        }
        return new RelationShape(renamed);
    }

    private RelationShape ShapeOfSpecification(QuerySpecification specification)
    {
        FragmentNode node = _tree.GetNode(specification);
        SourceScope scope = GetScope(node) ?? new SourceScope([]);

        List<ColumnInfo> columns = [];
        foreach (SelectElement element in specification.SelectElements)
        {
            switch (element)
            {
                case SelectScalarExpression scalar:
                    columns.Add(
                        new ColumnInfo(ColumnNameOf(scalar), _inferrer.Infer(scalar.Expression))
                    );
                    break;
                case SelectStarExpression star:
                    AddStarColumns(scope, star, columns);
                    break;
            }
        }
        return new RelationShape(columns);
    }

    private static string ColumnNameOf(SelectScalarExpression scalar)
    {
        if (scalar.ColumnName is not null)
            return scalar.ColumnName.Value;
        if (
            scalar.Expression is ColumnReferenceExpression reference
            && reference.MultiPartIdentifier.Count > 0
        )
            return reference
                .MultiPartIdentifier
                .Identifiers[reference.MultiPartIdentifier.Count - 1]
                .Value;
        return string.Empty;
    }

    private static void AddStarColumns(
        SourceScope scope,
        SelectStarExpression star,
        List<ColumnInfo> columns
    )
    {
        string? qualifier = star.Qualifier is { Count: > 0 }
            ? star.Qualifier.Identifiers[star.Qualifier.Count - 1].Value
            : null;

        foreach (RelationSource source in scope.Sources)
        {
            if (
                qualifier is not null
                && !string.Equals(source.Name, qualifier, StringComparison.OrdinalIgnoreCase)
            )
                continue;
            if (source.Shape is not null)
                columns.AddRange(source.Shape.Columns);
        }
    }
}
