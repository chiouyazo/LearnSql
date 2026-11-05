using LearnSql.Sql.Extensions;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql;

public static class MsSqlValidator
{
    public static IList<ParseError> Validate(string sql, SqlVersion version)
    {
        TSqlParser sqlParser = TSqlParser.CreateParser(version, false);
        IList<ParseError> errorList;
        using (StringReader rdr = new StringReader(sql))
        {
            sqlParser.Parse(rdr, out errorList);
        }

        if (!errorList.Any())
            return [];

        return errorList;
    }
}
