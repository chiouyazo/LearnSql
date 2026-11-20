using LearnSql.Sql.Dialect.Diagnostics;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Syntax;

public sealed class FragmentLocator
{
    private readonly SourceText _source;
    private readonly int _batchStartOffset;

    public FragmentLocator(SourceText source, int batchStartOffset)
    {
        _source = source;
        _batchStartOffset = batchStartOffset;
    }

    public SourceSpan GetSpan(TSqlFragment fragment)
    {
        return GetSpan(fragment.StartOffset, fragment.FragmentLength);
    }

    public SourceSpan GetSpan(int batchRelativeOffset, int length)
    {
        return _source.GetSpan(_batchStartOffset + batchRelativeOffset, length);
    }
}
