using LearnSql.Sql.Resources;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Unsupported;

public sealed record UnsupportedConstruct(
    string Id,
    Func<string> NameSource,
    Func<string> AlternativeSource,
    Func<TSqlFragment, bool> Matches,
    Func<TSqlFragment, string?>? Suggest = null
)
{
    public string Name => NameSource();

    public string Alternative => AlternativeSource();

    public static UnsupportedConstruct For<TFragment>(
        string id,
        string name,
        string alternative,
        Func<TFragment, bool>? predicate = null
    )
        where TFragment : TSqlFragment
    {
        return new UnsupportedConstruct(
            id,
            () => name,
            () => alternative,
            fragment => fragment is TFragment typed && (predicate is null || predicate(typed))
        );
    }

    public static UnsupportedConstruct ForAny(
        string id,
        string name,
        string alternative,
        params Type[] fragmentTypes
    )
    {
        return ForAny(id, name, alternative, null, fragmentTypes);
    }

    public static UnsupportedConstruct ForAny(
        string id,
        string name,
        string alternative,
        Func<TSqlFragment, string?>? suggest,
        params Type[] fragmentTypes
    )
    {
        return new UnsupportedConstruct(
            id,
            () => name,
            () => alternative,
            fragment => fragmentTypes.Any(type => type.IsInstanceOfType(fragment)),
            suggest
        );
    }

    internal static UnsupportedConstruct For<TFragment>(
        string id,
        Func<TFragment, bool>? predicate = null,
        params object?[] alternativeArguments
    )
        where TFragment : TSqlFragment
    {
        return Localized(
            id,
            fragment => fragment is TFragment typed && (predicate is null || predicate(typed)),
            null,
            alternativeArguments
        );
    }

    internal static UnsupportedConstruct ForAny(string id, params Type[] fragmentTypes)
    {
        return ForAny(id, null, fragmentTypes);
    }

    internal static UnsupportedConstruct ForAny(
        string id,
        Func<TSqlFragment, string?>? suggest,
        params Type[] fragmentTypes
    )
    {
        return Localized(
            id,
            fragment => fragmentTypes.Any(type => type.IsInstanceOfType(fragment)),
            suggest
        );
    }

    internal static UnsupportedConstruct Localized(
        string id,
        Func<TSqlFragment, bool> matches,
        Func<TSqlFragment, string?>? suggest = null,
        params object?[] alternativeArguments
    )
    {
        return new UnsupportedConstruct(
            id,
            () => UnsupportedStrings.Name(id),
            () => UnsupportedStrings.Alternative(id, alternativeArguments),
            matches,
            suggest
        );
    }
}
