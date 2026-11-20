using LearnSql.Sql.Dialect.Functions;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Dialect.Typing;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Unsupported;

public static class UnsupportedConstructs
{
    private static readonly string[] SupportedCollations = ["NOCASE", "BINARY", "RTRIM"];

    private static readonly string[] JsonFunctionNames =
    [
        "JSON_VALUE",
        "JSON_QUERY",
        "JSON_MODIFY",
        "JSON_OBJECT",
        "JSON_ARRAY",
        "ISJSON",
        "JSON_PATH_EXISTS",
    ];

    private static readonly Type[] SupportedStatementTypes =
    [
        typeof(SelectStatement),
        typeof(InsertStatement),
        typeof(UpdateStatement),
        typeof(DeleteStatement),
        typeof(TruncateTableStatement),
        typeof(CreateTableStatement),
        typeof(DropTableStatement),
        typeof(AlterTableAddTableElementStatement),
        typeof(AlterTableDropTableElementStatement),
        typeof(CreateViewStatement),
        typeof(DropViewStatement),
        typeof(CreateIndexStatement),
        typeof(DropIndexStatement),
        typeof(BeginTransactionStatement),
        typeof(CommitTransactionStatement),
        typeof(RollbackTransactionStatement),
        typeof(SaveTransactionStatement),
        typeof(DeclareVariableStatement),
        typeof(SetVariableStatement),
        typeof(PredicateSetStatement),
        typeof(SetTransactionIsolationLevelStatement),
    ];

    public static IReadOnlyList<UnsupportedConstruct> CreateDefault()
    {
        return
        [
            UnsupportedConstruct.For<UnqualifiedJoin>(
                "CrossOuterApply",
                join => join.UnqualifiedJoinType != UnqualifiedJoinType.CrossJoin
            ),
            UnsupportedConstruct.For<PivotedTableReference>("Pivot"),
            UnsupportedConstruct.For<UnpivotedTableReference>("Unpivot"),
            UnsupportedConstruct.For<MergeStatement>("Merge"),
            UnsupportedConstruct.For<TryCatchStatement>("TryCatch"),
            UnsupportedConstruct.ForAny(
                "While",
                typeof(WhileStatement),
                typeof(BreakStatement),
                typeof(ContinueStatement)
            ),
            UnsupportedConstruct.ForAny(
                "ControlFlow",
                typeof(IfStatement),
                typeof(BeginEndBlockStatement),
                typeof(GoToStatement),
                typeof(LabelStatement),
                typeof(ReturnStatement),
                typeof(WaitForStatement)
            ),
            UnsupportedConstruct.ForAny(
                "PrintRaiseError",
                typeof(PrintStatement),
                typeof(RaiseErrorStatement),
                typeof(RaiseErrorLegacyStatement),
                typeof(ThrowStatement)
            ),
            UnsupportedConstruct.ForAny(
                "Cursor",
                typeof(DeclareCursorStatement),
                typeof(OpenCursorStatement),
                typeof(FetchCursorStatement),
                typeof(CloseCursorStatement),
                typeof(DeallocateCursorStatement)
            ),
            UnsupportedConstruct.ForAny(
                "TableVariable",
                typeof(DeclareTableVariableStatement),
                typeof(VariableTableReference)
            ),
            UnsupportedConstruct.For<SelectSetVariable>("SelectAssignment"),
            UnsupportedConstruct.ForAny(
                "StoredProcedure",
                typeof(CreateProcedureStatement),
                typeof(AlterProcedureStatement),
                typeof(CreateOrAlterProcedureStatement),
                typeof(DropProcedureStatement)
            ),
            UnsupportedConstruct.ForAny(
                "UserDefinedFunction",
                typeof(CreateFunctionStatement),
                typeof(AlterFunctionStatement),
                typeof(CreateOrAlterFunctionStatement),
                typeof(DropFunctionStatement)
            ),
            UnsupportedConstruct.For<FunctionCall>(
                "UserDefinedFunctionCall",
                call => call.CallTarget is not null
            ),
            UnsupportedConstruct.ForAny(
                "Trigger",
                typeof(CreateTriggerStatement),
                typeof(AlterTriggerStatement),
                typeof(CreateOrAlterTriggerStatement),
                typeof(DropTriggerStatement),
                typeof(EnableDisableTriggerStatement)
            ),
            UnsupportedConstruct.ForAny(
                "Execute",
                MistypedStatementSuggestion.ForImplicitExecute,
                typeof(ExecuteStatement),
                typeof(ExecuteInsertSource)
            ),
            UnsupportedConstruct.ForAny(
                "OutputClause",
                typeof(OutputClause),
                typeof(OutputIntoClause)
            ),
            UnsupportedConstruct.For<ForClause>("ForClause"),
            UnsupportedConstruct.ForAny(
                "TableValuedFunction",
                typeof(SchemaObjectFunctionTableReference),
                typeof(OpenJsonTableReference),
                typeof(OpenXmlTableReference),
                typeof(OpenQueryTableReference),
                typeof(OpenRowsetTableReference),
                typeof(BuiltInFunctionTableReference),
                typeof(GlobalFunctionTableReference),
                typeof(AdHocTableReference)
            ),
            UnsupportedConstruct.For<FunctionCall>(
                "JsonFunction",
                call =>
                    call.CallTarget is null
                    && JsonFunctionNames.Contains(
                        call.FunctionName.Value,
                        StringComparer.OrdinalIgnoreCase
                    )
            ),
            UnsupportedConstruct.ForAny(
                "TryConvert",
                typeof(TryCastCall),
                typeof(TryConvertCall),
                typeof(TryParseCall),
                typeof(ParseCall)
            ),
            UnsupportedConstruct.For<ConvertCall>(
                "ConvertStyle",
                call => call.Style is not null && !IsSupportedStyle(call.Style),
                string.Join(", ", ConvertStyleFormats.SupportedCodes)
            ),
            UnsupportedConstruct.For<SqlDataTypeReference>(
                "DataType",
                type => !TSqlDataTypes.IsSupported(type.SqlDataTypeOption)
            ),
            UnsupportedConstruct.ForAny(
                "UserDefinedType",
                typeof(UserDataTypeReference),
                typeof(XmlDataTypeReference)
            ),
            UnsupportedConstruct.For<ColumnDefinition>(
                "IdentityColumn",
                column => !IdentityColumns.IsTranslatable(column)
            ),
            UnsupportedConstruct.For<DropIndexStatement>(
                "DropIndexForm",
                statement => !DropIndexForms.IsTranslatable(statement)
            ),
            UnsupportedConstruct.For<AlterTableAlterColumnStatement>("AlterColumn"),
            UnsupportedConstruct.For<AlterTableAddTableElementStatement>(
                "AlterTableAddElement",
                statement =>
                    statement.Definition.TableConstraints.Count > 0
                    || statement.Definition.Indexes.Count > 0
                    || statement.Definition.ColumnDefinitions.Count != 1
            ),
            UnsupportedConstruct.For<AlterTableDropTableElementStatement>(
                "AlterTableDropElement",
                statement =>
                    statement.AlterTableDropTableElements.Any(element =>
                        element.TableElementType != TableElementType.Column
                    )
            ),
            UnsupportedConstruct.For<CreateIndexStatement>(
                "IndexOptions",
                statement =>
                    statement.IncludeColumns.Count > 0
                    || statement.IndexOptions.Count > 0
                    || statement.OnFileGroupOrPartitionScheme is not null
                    || statement.FileStreamOn is not null
            ),
            UnsupportedConstruct.ForAny(
                "AlterView",
                typeof(AlterViewStatement),
                typeof(CreateOrAlterViewStatement)
            ),
            UnsupportedConstruct.For<CreateViewStatement>(
                "ViewOptions",
                view => view.ViewOptions.Count > 0 || view.WithCheckOption
            ),
            UnsupportedConstruct.For<DropTableStatement>(
                "DropMultipleTables",
                statement => statement.Objects.Count > 1
            ),
            UnsupportedConstruct.For<StatementWithCtesAndXmlNamespaces>(
                "QueryHint",
                statement => statement.OptimizerHints.Count > 0
            ),
            UnsupportedConstruct.For<NamedTableReference>(
                "TableHint",
                reference =>
                    reference.TableHints.Count > 0
                    && !reference.TableHints.All(TableHintClassifier.IsIgnorable)
            ),
            UnsupportedConstruct.For<GroupByClause>(
                "GroupByOption",
                clause => clause.GroupByOption != GroupByOption.None
            ),
            UnsupportedConstruct.ForAny(
                "GroupingSets",
                typeof(RollupGroupingSpecification),
                typeof(CubeGroupingSpecification),
                typeof(GroupingSetsGroupingSpecification)
            ),
            UnsupportedConstruct.For<QueryDerivedTable>(
                "DerivedTableColumnList",
                table => table.Columns.Count > 0
            ),
            UnsupportedConstruct.For<InlineDerivedTable>("ValuesTable"),
            UnsupportedConstruct.For<SubqueryComparisonPredicate>("AnyAll"),
            UnsupportedConstruct.ForAny(
                "FullText",
                typeof(FullTextPredicate),
                typeof(FullTextTableReference)
            ),
            UnsupportedConstruct.For<LikePredicate>(
                "LikeCharacterClass",
                like =>
                    like.SecondExpression is StringLiteral pattern && pattern.Value.Contains('[')
            ),
            UnsupportedConstruct.For<PrimaryExpression>(
                "WindowsCollation",
                expression =>
                    expression.Collation is not null
                    && !IsSupportedCollation(expression.Collation.Value)
            ),
            UnsupportedConstruct.For<ColumnDefinitionBase>(
                "ColumnCollation",
                column =>
                    column.Collation is not null && !IsSupportedCollation(column.Collation.Value)
            ),
            UnsupportedConstruct.For<UpdateSpecification>(
                "UpdateFrom",
                update => update.FromClause is not null
            ),
            UnsupportedConstruct.For<DeleteSpecification>(
                "DeleteFrom",
                delete => delete.FromClause is not null
            ),
            UnsupportedConstruct.For<DataModificationSpecification>(
                "TopInModification",
                specification => specification.TopRowFilter is not null
            ),
            UnsupportedConstruct.For<TopRowFilter>(
                "TopPercentOrTies",
                top => top.Percent || top.WithTies
            ),
            UnsupportedConstruct.For<BinaryExpression>(
                "BitwiseXor",
                expression => expression.BinaryExpressionType == BinaryExpressionType.BitwiseXor
            ),
            UnsupportedConstruct.For<SetVariableStatement>(
                "BitwiseXorAssignment",
                statement => statement.AssignmentKind == AssignmentKind.BitwiseXorEquals
            ),
            UnsupportedConstruct.For<GlobalVariableExpression>(
                "SystemVariable",
                variable =>
                    !string.Equals(variable.Name, "@@IDENTITY", StringComparison.OrdinalIgnoreCase)
            ),
            UnsupportedConstruct.For<ParameterlessCall>(
                "UserFunction",
                call =>
                    call.ParameterlessCallType != ParameterlessCallType.CurrentTimestamp
                    && call.ParameterlessCallType != ParameterlessCallType.CurrentDate
            ),
            UnsupportedConstruct.ForAny(
                "Sequence",
                typeof(NextValueForExpression),
                typeof(CreateSequenceStatement),
                typeof(AlterSequenceStatement),
                typeof(DropSequenceStatement)
            ),
            UnsupportedConstruct.For<SchemaObjectName>(
                "ForeignSchema",
                name => IsForeignQualified(name)
            ),
            UnsupportedConstruct.For<BeginTransactionStatement>(
                "NamedTransaction",
                statement =>
                    statement.Name is not null || statement.Distributed || statement.MarkDefined
            ),
            UnsupportedConstruct.For<CommitTransactionStatement>(
                "NamedCommit",
                statement => statement.Name is not null
            ),
            UnsupportedConstruct.For<RollbackTransactionStatement>(
                "RollbackVariableName",
                statement => statement.Name is not null && statement.Name.Identifier is null
            ),
            UnsupportedConstruct.For<SaveTransactionStatement>(
                "SavepointName",
                statement => statement.Name?.Identifier is null
            ),
            UnsupportedConstruct.For<PredicateSetStatement>(
                "SetOption",
                statement => !SetStatementClassifier.IsIgnorable(statement)
            ),
            UnsupportedConstruct.ForAny(
                "SetCommand",
                typeof(SetCommandStatement),
                typeof(SetIdentityInsertStatement),
                typeof(SetRowCountStatement),
                typeof(SetTextSizeStatement),
                typeof(SetStatisticsStatement),
                typeof(SetErrorLevelStatement),
                typeof(SetUserStatement)
            ),
            UnsupportedConstruct.For<UseStatement>("Use"),
            UnsupportedConstruct.For<CreateSchemaStatement>("CreateSchema"),
            UnsupportedConstruct.Localized(
                "UnsupportedStatement",
                fragment => fragment is TSqlStatement && !IsSupportedStatement(fragment)
            ),
        ];
    }

    private static bool IsSupportedStatement(TSqlFragment fragment)
    {
        return SupportedStatementTypes.Any(type => type.IsInstanceOfType(fragment));
    }

    private static bool IsSupportedCollation(string collation)
    {
        return SupportedCollations.Contains(collation, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsSupportedStyle(ScalarExpression style)
    {
        return style is IntegerLiteral literal
            && int.TryParse(literal.Value, out int code)
            && ConvertStyleFormats.IsSupported(code);
    }

    private static bool IsForeignQualified(SchemaObjectName name)
    {
        if (name.ServerIdentifier is not null || name.DatabaseIdentifier is not null)
            return true;

        return name.SchemaIdentifier is not null
            && !SandboxSchemas.IsLocal(name.SchemaIdentifier.Value);
    }
}
