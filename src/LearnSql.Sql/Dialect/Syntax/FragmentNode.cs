using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Syntax;

public sealed class FragmentNode
{
    public FragmentNode(TSqlFragment fragment, FragmentNode? parent)
    {
        Fragment = fragment;
        Parent = parent;
    }

    public TSqlFragment Fragment { get; }

    public FragmentNode? Parent { get; }

    public int Start => Fragment.StartOffset;

    public int End => Fragment.StartOffset + Fragment.FragmentLength;

    public IEnumerable<FragmentNode> Ancestors()
    {
        FragmentNode? current = Parent;
        while (current is not null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    public FragmentNode? FindAncestor<TFragment>()
        where TFragment : TSqlFragment
    {
        foreach (FragmentNode ancestor in Ancestors())
        {
            if (ancestor.Fragment is TFragment)
                return ancestor;
        }
        return null;
    }
}
