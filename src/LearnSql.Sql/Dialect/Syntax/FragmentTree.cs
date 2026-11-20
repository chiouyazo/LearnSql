using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Syntax;

public sealed class FragmentTree
{
    private readonly Dictionary<TSqlFragment, FragmentNode> _byFragment;

    public FragmentTree(TSqlFragment root, IReadOnlyList<FragmentNode> nodesInSourceOrder)
    {
        Root = root;
        Nodes = nodesInSourceOrder;
        _byFragment = new Dictionary<TSqlFragment, FragmentNode>(
            ReferenceEqualityComparer.Instance
        );
        foreach (FragmentNode node in nodesInSourceOrder)
            _byFragment[node.Fragment] = node;
    }

    public TSqlFragment Root { get; }

    public IReadOnlyList<FragmentNode> Nodes { get; }

    public IList<TSqlParserToken> Tokens => Root.ScriptTokenStream;

    public FragmentNode GetNode(TSqlFragment fragment)
    {
        return _byFragment[fragment];
    }

    public bool TryGetNode(TSqlFragment fragment, out FragmentNode? node)
    {
        return _byFragment.TryGetValue(fragment, out node);
    }

    public static FragmentTree Build(TSqlFragment root)
    {
        FragmentCollector collector = new FragmentCollector();
        root.Accept(collector);
        return new FragmentTree(root, collector.GetNodesInSourceOrder());
    }

    private sealed class FragmentCollector : TSqlFragmentVisitor
    {
        private readonly List<FragmentNode> _nodes = [];
        private readonly Stack<FragmentNode> _open = new Stack<FragmentNode>();

        public override void Visit(TSqlFragment fragment)
        {
            if (fragment.FirstTokenIndex < 0)
            {
                _nodes.Add(new FragmentNode(fragment, _open.Count > 0 ? _open.Peek() : null));
                return;
            }

            while (_open.Count > 0 && !Contains(_open.Peek().Fragment, fragment))
                _open.Pop();

            FragmentNode node = new FragmentNode(fragment, _open.Count > 0 ? _open.Peek() : null);
            _nodes.Add(node);
            _open.Push(node);
        }

        public IReadOnlyList<FragmentNode> GetNodesInSourceOrder()
        {
            return _nodes.OrderBy(node => node.Fragment.StartOffset).ToList();
        }

        private static bool Contains(TSqlFragment outer, TSqlFragment inner)
        {
            return outer.FirstTokenIndex <= inner.FirstTokenIndex
                && inner.LastTokenIndex <= outer.LastTokenIndex;
        }
    }
}
