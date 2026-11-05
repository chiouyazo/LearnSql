using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Extensions;

public static class ParseErrorExtensions
{
	public static string ToSummary(this ParseError parseError)
	{
		return $"L: {parseError.Line}:{parseError.Column} [{parseError.Number}] {parseError.Message}";
	}
}