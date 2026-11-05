using LearnSql.Sql.Generated;

namespace LearnSql.Sql.MsSql;

public class MsSqlErrorMessages : GenericErrorMessages
{
    public override string NoSuchTable() => @"Invalid object name '{Table}'.";

    public override string NoTablesSpecified() => "Must specify table to select from.";
}
