using LearnSql.Sql.Dialect.Batching;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Parsing;
using LearnSql.Sql.Dialect.Rewriting;
using LearnSql.Sql.Dialect.Rewriting.Rules;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Dialect.Typing;
using LearnSql.Sql.Dialect.Unsupported;
using LearnSql.Sql.Resources;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect;

public sealed class TSqlDialectTranslator
{
    private readonly BatchSplitter _splitter = new BatchSplitter();
    private readonly MsSqlParser _parser;
    private readonly UnsupportedConstructDetector _detector;
    private readonly TranslationEngine _engine;
    private readonly ISchemaCatalog _schema;
    private readonly StatementProfiler _profiler = new StatementProfiler();

    public TSqlDialectTranslator(
        ISchemaCatalog schema,
        IReadOnlyList<UnsupportedConstruct> unsupportedConstructs,
        IReadOnlyList<ITranslationRule> rules,
        SqlVersion version = SqlVersion.Sql180
    )
    {
        _schema = schema;
        _parser = new MsSqlParser(version);
        _detector = new UnsupportedConstructDetector(unsupportedConstructs);
        _engine = new TranslationEngine(rules);
    }

    public static TSqlDialectTranslator CreateDefault(ISchemaCatalog? schema = null)
    {
        return new TSqlDialectTranslator(
            schema ?? SchemaCatalog.Empty,
            UnsupportedConstructs.CreateDefault(),
            DefaultTranslationRules.Create()
        );
    }

    public DialectTranslationResult Translate(string sql)
    {
        SourceText source = new SourceText(sql);
        ParseDiagnosticFactory parseDiagnostics = new ParseDiagnosticFactory(source);
        List<DialectDiagnostic> diagnostics = [];
        List<TranslatedBatch> batches = [];

        foreach (SqlBatchSource batchSource in _splitter.Split(sql))
        {
            ParsedBatch parsed = _parser.Parse(batchSource, parseDiagnostics);
            diagnostics.AddRange(parsed.Diagnostics);
            if (parsed.HasErrors)
                continue;

            TranslatedBatch? translated = TranslateBatch(parsed, source, diagnostics);
            if (translated is not null)
                batches.Add(translated);
        }

        bool success = !diagnostics.Any(diagnostic =>
            diagnostic.Severity == DiagnosticSeverity.Error
        );
        return new DialectTranslationResult(success, success ? batches : [], diagnostics);
    }

    private TranslatedBatch? TranslateBatch(
        ParsedBatch parsed,
        SourceText source,
        List<DialectDiagnostic> diagnostics
    )
    {
        FragmentTree tree = FragmentTree.Build(parsed.Script);
        FragmentLocator locator = new FragmentLocator(source, parsed.Source.StartOffset);

        IReadOnlyList<DialectDiagnostic> unsupported = _detector.Detect(tree, locator);
        diagnostics.AddRange(unsupported);
        if (unsupported.Count > 0)
            return null;

        TranslationContext context = new TranslationContext(
            tree,
            parsed.Source.Text,
            locator,
            _schema
        );
        try
        {
            _engine.Run(context);
            diagnostics.AddRange(context.Diagnostics);
            if (
                context.Diagnostics.Any(diagnostic =>
                    diagnostic.Severity == DiagnosticSeverity.Error
                )
            )
                return null;

            return CreateBatch(parsed, context, locator);
        }
        catch (InvalidOperationException exception)
        {
            diagnostics.Add(CreateInternalError(exception, locator, parsed));
            return null;
        }
    }

    private static DialectDiagnostic CreateInternalError(
        InvalidOperationException exception,
        FragmentLocator locator,
        ParsedBatch parsed
    )
    {
        return new DialectDiagnostic(
            DiagnosticKind.TranslationError,
            DiagnosticSeverity.Error,
            "InternalTranslationError",
            DialectStrings.InternalTranslationError,
            exception.Message,
            locator.GetSpan(0, parsed.Source.Text.Length),
            null
        );
    }

    private TranslatedBatch CreateBatch(
        ParsedBatch parsed,
        TranslationContext context,
        FragmentLocator locator
    )
    {
        List<TranslatedStatement> statements = [];
        foreach (TSqlBatch batch in parsed.Script.Batches)
        {
            foreach (TSqlStatement statement in batch.Statements)
            {
                string sql = TrimStatement(context.Render(statement));
                if (sql.Length > 0)
                    statements.Add(
                        new TranslatedStatement(
                            sql,
                            locator.GetSpan(statement),
                            false,
                            _profiler.Profile(statement)
                        )
                    );
            }
        }

        if (context.UsesVariables && statements.Count > 0)
        {
            SourceSpan origin = statements[0].Origin;
            IEnumerable<TranslatedStatement> setup = VariableStorage.SetupStatements.Select(
                sql => new TranslatedStatement(sql, origin, true)
            );
            statements.InsertRange(0, setup);
        }

        SourceSpan batchSpan = locator.GetSpan(0, parsed.Source.Text.Length);
        return new TranslatedBatch(statements, parsed.Source.RepeatCount, batchSpan);
    }

    private static string TrimStatement(string sql)
    {
        return sql.Trim().TrimEnd(';').TrimEnd();
    }
}
