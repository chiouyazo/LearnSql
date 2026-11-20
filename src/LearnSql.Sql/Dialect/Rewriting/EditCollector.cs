using System.Text;

namespace LearnSql.Sql.Dialect.Rewriting;

public sealed class EditCollector
{
    private readonly string _source;
    private readonly List<TextEdit> _edits = [];
    private int _sequence;

    public EditCollector(string source)
    {
        _source = source;
    }

    public string Source => _source;

    public void Replace(int start, int length, string text)
    {
        Replace(start, length, () => text);
    }

    public void Replace(int start, int length, Func<string> text)
    {
        Add(start, length, EditPlacement.Replacement, text);
    }

    public void Delete(int start, int length)
    {
        Replace(start, length, string.Empty);
    }

    public void InsertBefore(int offset, string text)
    {
        InsertBefore(offset, () => text);
    }

    public void InsertBefore(int offset, Func<string> text)
    {
        Add(offset, 0, EditPlacement.InsertBefore, text);
    }

    public void InsertAfter(int offset, string text)
    {
        InsertAfter(offset, () => text);
    }

    public void InsertAfter(int offset, Func<string> text)
    {
        Add(offset, 0, EditPlacement.InsertAfter, text);
    }

    public string Render(int start, int length)
    {
        return Apply(start, length);
    }

    private void Add(int start, int length, EditPlacement placement, Func<string> text)
    {
        if (start < 0 || length < 0 || start + length > _source.Length)
            throw new ArgumentOutOfRangeException(
                nameof(start),
                "The edit range lies outside the source text."
            );

        _edits.Add(new TextEdit(start, length, placement, _sequence++, text));
    }

    private string Apply(int start, int length)
    {
        int end = start + length;
        List<TextEdit> contained = _edits
            .Where(edit => edit.Start >= start && edit.End <= end)
            .OrderBy(edit => edit.Start)
            .ThenBy(edit => PlacementOrder(edit.Placement))
            .ThenBy(edit =>
                edit.Placement == EditPlacement.InsertAfter ? -edit.Sequence : edit.Sequence
            )
            .ThenByDescending(edit => edit.Length)
            .ToList();

        StringBuilder builder = new StringBuilder();
        int position = start;
        int appliedEnd = start;

        foreach (TextEdit edit in contained)
        {
            if (edit.Start < appliedEnd)
            {
                if (edit.End > appliedEnd)
                    throw new InvalidOperationException(
                        "Zwei Bearbeitungen ueberlappen sich teilweise."
                    );
                continue;
            }

            builder.Append(_source, position, edit.Start - position);
            builder.Append(edit.Text());
            position = edit.End;
            appliedEnd = Math.Max(appliedEnd, edit.End);
        }

        builder.Append(_source, position, end - position);
        return builder.ToString();
    }

    private static int PlacementOrder(EditPlacement placement)
    {
        return placement switch
        {
            EditPlacement.InsertAfter => 0,
            EditPlacement.InsertBefore => 1,
            _ => 2,
        };
    }
}
