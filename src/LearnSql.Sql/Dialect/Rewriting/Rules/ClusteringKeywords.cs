using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public static class ClusteringKeywords
{
    public static void RemoveFrom(TranslationContext context, TSqlFragment fragment)
    {
        for (int index = fragment.FirstTokenIndex; index <= fragment.LastTokenIndex; index++)
        {
            TSqlParserToken token = context.TokenAt(index);
            if (token.TokenType is not (TSqlTokenType.Clustered or TSqlTokenType.NonClustered))
                continue;

            int following = context.NextSignificantTokenIndex(index);
            context.DeleteRange(token.Offset, context.TokenAt(following).Offset);
            return;
        }
    }
}
