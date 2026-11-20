namespace LearnSql.Sql.Dialect.Diagnostics;

public sealed class SourceText
{
    private readonly int[] _lineStarts;

    public SourceText(string text)
    {
        Text = text;
        _lineStarts = ComputeLineStarts(text);
    }

    public string Text { get; }

    public int Length => Text.Length;

    public SourcePosition GetPosition(int offset)
    {
        int clamped = Math.Clamp(offset, 0, Text.Length);
        int index = Array.BinarySearch(_lineStarts, clamped);
        if (index < 0)
            index = ~index - 1;
        return new SourcePosition(clamped, index + 1, clamped - _lineStarts[index] + 1);
    }

    public SourceSpan GetSpan(int offset, int length)
    {
        return new SourceSpan(GetPosition(offset), GetPosition(offset + Math.Max(length, 0)));
    }

    private static int[] ComputeLineStarts(string text)
    {
        List<int> starts = [0];
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\r')
            {
                if (i + 1 < text.Length && text[i + 1] == '\n')
                    i++;
                starts.Add(i + 1);
            }
            else if (text[i] == '\n')
            {
                starts.Add(i + 1);
            }
        }
        return starts.ToArray();
    }
}
