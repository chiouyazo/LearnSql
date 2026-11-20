using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public static class OperatorTokens
{
    public static IReadOnlyList<TSqlParserToken> Between(
        TranslationContext context,
        TSqlFragment left,
        TSqlFragment right
    )
    {
        List<TSqlParserToken> tokens = [];
        for (int index = left.LastTokenIndex + 1; index < right.FirstTokenIndex; index++)
        {
            TSqlParserToken token = context.TokenAt(index);
            if (
                token.TokenType
                is not (
                    TSqlTokenType.WhiteSpace
                    or TSqlTokenType.SingleLineComment
                    or TSqlTokenType.MultilineComment
                )
            )
            {
                tokens.Add(token);
            }
        }
        return tokens;
    }

    public static TSqlParserToken? FindSingle(
        TranslationContext context,
        TSqlFragment left,
        TSqlFragment right,
        TSqlTokenType type
    )
    {
        IReadOnlyList<TSqlParserToken> tokens = Between(context, left, right);
        return tokens.Count == 1 && tokens[0].TokenType == type ? tokens[0] : null;
    }
}
