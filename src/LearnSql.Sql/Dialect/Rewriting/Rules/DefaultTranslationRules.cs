namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public static class DefaultTranslationRules
{
    public static IReadOnlyList<ITranslationRule> Create()
    {
        return
        [
            new IdentifierRule(),
            new SchemaObjectNameRule(),
            new ColumnSchemaQualifierRule(),
            new NationalStringLiteralRule(),
            new BinaryLiteralRule(),
            new TopRowFilterRule(),
            new TopWithOffsetRule(),
            new OffsetFetchRule(),
            new ParenthesizedQueryRule(),
            new IntersectPrecedenceRule(),
            new StringConcatenationRule(),
            new DateArithmeticRule(),
            new DivisionRule(),
            new NegatedComparisonRule(),
            new ReservedFunctionNameRule(),
            new LeftRightFunctionRule(),
            new DatePartArgumentRule(),
            new IsNullCoercionRule(),
            new ScopeIdentityRule(),
            new GlobalIdentityRule(),
            new CurrentTimestampRule(),
            new StringAggWithinGroupRule(),
            new CastCallRule(),
            new ConvertCallRule(),
            new DataTypeMaxRule(),
            new DeclareVariableRule(),
            new SetVariableRule(),
            new VariableReferenceRule(),
            new VariableColumnNameRule(),
            new SelectIntoRule(),
            new TemporaryTableRule(),
            new TruncateTableRule(),
            new IgnoredSetStatementRule(),
            new BeginTransactionRule(),
            new CommitTransactionRule(),
            new RollbackTransactionRule(),
            new SaveTransactionRule(),
            new AliasAssignmentRule(),
            new TableHintRule(),
            new CreateIndexRule(),
            new PrimaryKeyClusteringRule(),
            new DropIndexRule(),
            new IdentityColumnRule(),
            new DefaultExpressionRule(),
            new DeleteWithoutFromRule(),
            new InsertWithoutIntoRule(),
        ];
    }
}
