using LearnSql.Sql.Execution;

namespace LearnSql.Tests.Execution;

public class SandboxAuthorizationPolicyTests
{
    private const int SelectCode = 21;
    private const int ReadCode = 20;
    private const int CreateTableCode = 2;
    private const int TransactionCode = 22;

    private readonly SandboxAuthorizationPolicy _policy = new SandboxAuthorizationPolicy();

    private AuthorizationDecision Decide(
        AuthorizerAction action,
        string? first = null,
        string? second = null
    )
    {
        return _policy.Decide((int)action, first, second);
    }

    [Fact]
    public void AuthorizerActionCodes_MatchTheSqliteConstants()
    {
        Assert.Equal(9, (int)AuthorizerAction.Delete);
        Assert.Equal(18, (int)AuthorizerAction.Insert);
        Assert.Equal(19, (int)AuthorizerAction.Pragma);
        Assert.Equal(23, (int)AuthorizerAction.Update);
        Assert.Equal(24, (int)AuthorizerAction.Attach);
        Assert.Equal(25, (int)AuthorizerAction.Detach);
        Assert.Equal(31, (int)AuthorizerAction.Function);
    }

    [Fact]
    public void Decide_AttachAndDetach_AreDenied()
    {
        Assert.Equal(AuthorizationDecision.Deny, Decide(AuthorizerAction.Attach, "x.db"));
        Assert.Equal(AuthorizationDecision.Deny, Decide(AuthorizerAction.Detach, "other"));
        Assert.Equal(AuthorizationDecision.Deny, Decide(AuthorizerAction.Attach));
    }

    [Theory]
    [InlineData("table_info")]
    [InlineData("table_xinfo")]
    [InlineData("TABLE_INFO")]
    [InlineData("Table_XInfo")]
    public void Decide_IntrospectionPragmas_AreAllowed(string pragma)
    {
        Assert.Equal(AuthorizationDecision.Allow, Decide(AuthorizerAction.Pragma, pragma));
    }

    [Theory]
    [InlineData("foreign_keys")]
    [InlineData("writable_schema")]
    [InlineData("journal_mode")]
    [InlineData("max_page_count")]
    [InlineData("table_list")]
    [InlineData("foreign_key_list")]
    [InlineData("")]
    public void Decide_AllOtherPragmas_AreDenied(string pragma)
    {
        Assert.Equal(AuthorizationDecision.Deny, Decide(AuthorizerAction.Pragma, pragma));
    }

    [Fact]
    public void Decide_PragmaWithoutName_IsDenied()
    {
        Assert.Equal(AuthorizationDecision.Deny, Decide(AuthorizerAction.Pragma));
    }

    [Theory]
    [InlineData("sqlite_master")]
    [InlineData("sqlite_schema")]
    [InlineData("sqlite_temp_master")]
    [InlineData("sqlite_temp_schema")]
    [InlineData("SQLITE_MASTER")]
    public void Decide_WritesToTheSchemaTables_AreAllowed(string table)
    {
        Assert.Equal(AuthorizationDecision.Allow, Decide(AuthorizerAction.Insert, table));
        Assert.Equal(AuthorizationDecision.Allow, Decide(AuthorizerAction.Update, table));
        Assert.Equal(AuthorizationDecision.Allow, Decide(AuthorizerAction.Delete, table));
    }

    [Theory]
    [InlineData("sqlite_sequence")]
    [InlineData("SQLITE_SEQUENCE")]
    public void Decide_SequenceTable_AllowsOnlyDelete(string table)
    {
        Assert.Equal(AuthorizationDecision.Allow, Decide(AuthorizerAction.Delete, table));
        Assert.Equal(AuthorizationDecision.Deny, Decide(AuthorizerAction.Insert, table));
        Assert.Equal(AuthorizationDecision.Deny, Decide(AuthorizerAction.Update, table));
    }

    [Theory]
    [InlineData("sqlite_stat1")]
    [InlineData("sqlite_stat4")]
    [InlineData("sqlite_autoindex_T_1")]
    [InlineData("sqlite_whatever")]
    [InlineData("SQLITE_STAT1")]
    public void Decide_OtherInternalTables_AreDeniedForEveryWrite(string table)
    {
        Assert.Equal(AuthorizationDecision.Deny, Decide(AuthorizerAction.Insert, table));
        Assert.Equal(AuthorizationDecision.Deny, Decide(AuthorizerAction.Update, table));
        Assert.Equal(AuthorizationDecision.Deny, Decide(AuthorizerAction.Delete, table));
    }

    [Theory]
    [InlineData("Customer")]
    [InlineData("T")]
    [InlineData("sqlit")]
    [InlineData("my_sqlite_table")]
    [InlineData("__learnsql_variables")]
    public void Decide_OrdinaryTables_AreAllowedForEveryWrite(string table)
    {
        Assert.Equal(AuthorizationDecision.Allow, Decide(AuthorizerAction.Insert, table));
        Assert.Equal(AuthorizationDecision.Allow, Decide(AuthorizerAction.Update, table));
        Assert.Equal(AuthorizationDecision.Allow, Decide(AuthorizerAction.Delete, table));
    }

    [Theory]
    [InlineData(AuthorizerAction.Insert)]
    [InlineData(AuthorizerAction.Update)]
    [InlineData(AuthorizerAction.Delete)]
    public void Decide_WriteWithoutTableName_IsAllowed(AuthorizerAction action)
    {
        Assert.Equal(AuthorizationDecision.Allow, Decide(action));
    }

    [Theory]
    [InlineData("load_extension")]
    [InlineData("LOAD_EXTENSION")]
    [InlineData("fts3_tokenizer")]
    [InlineData("Fts3_Tokenizer")]
    public void Decide_BlockedFunctions_AreDeniedBySecondArgument(string function)
    {
        Assert.Equal(AuthorizationDecision.Deny, Decide(AuthorizerAction.Function, null, function));
    }

    [Theory]
    [InlineData("abs")]
    [InlineData("count")]
    [InlineData("substr")]
    [InlineData("LEN")]
    public void Decide_OrdinaryFunctions_AreAllowed(string function)
    {
        Assert.Equal(
            AuthorizationDecision.Allow,
            Decide(AuthorizerAction.Function, null, function)
        );
    }

    [Fact]
    public void Decide_FunctionNameInTheFirstArgument_DoesNotCount()
    {
        Assert.Equal(
            AuthorizationDecision.Allow,
            Decide(AuthorizerAction.Function, "load_extension", "abs")
        );
        Assert.Equal(AuthorizationDecision.Allow, Decide(AuthorizerAction.Function));
    }

    [Theory]
    [InlineData(SelectCode)]
    [InlineData(ReadCode)]
    [InlineData(CreateTableCode)]
    [InlineData(TransactionCode)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(9999)]
    public void Decide_UnlistedActionCodes_AreAllowed(int code)
    {
        Assert.Equal(AuthorizationDecision.Allow, _policy.Decide(code, "sqlite_master", "x"));
    }

    [Fact]
    public void AuthorizationDecision_UsesTheSqliteResultCodes()
    {
        Assert.Equal(0, (int)AuthorizationDecision.Allow);
        Assert.Equal(1, (int)AuthorizationDecision.Deny);
    }
}
