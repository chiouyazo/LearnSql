using System.Net;
using System.Text;
using LearnSql.Core.Services.Guided;
using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Learning.Guided;

public partial class GuidedSqlView
{
    private string _renderedSql = string.Empty;
    private MarkupString _markup;

    [Parameter, EditorRequired]
    public string Sql { get; set; } = string.Empty;

    [Inject]
    private IStringLocalizer<GuidedStrings> L { get; set; } = default!;

    protected override void OnParametersSet()
    {
        if (_renderedSql == Sql && _markup.Value is not null)
        {
            return;
        }

        _renderedSql = Sql;
        _markup = new MarkupString(Render(Sql));
    }

    private static string Render(string sql)
    {
        StringBuilder builder = new StringBuilder();
        foreach (GuidedSqlToken token in GuidedSqlTokenizer.Tokenize(sql))
        {
            string text = WebUtility.HtmlEncode(token.Text);
            string? css = CssFor(token.Kind);
            if (css is null)
            {
                builder.Append(text);
            }
            else
            {
                builder
                    .Append("<span class=\"")
                    .Append(css)
                    .Append("\">")
                    .Append(text)
                    .Append("</span>");
            }
        }

        return builder.ToString();
    }

    private static string? CssFor(GuidedSqlTokenKind kind)
    {
        return kind switch
        {
            GuidedSqlTokenKind.Keyword => "tok-keyword",
            GuidedSqlTokenKind.Function => "tok-function",
            GuidedSqlTokenKind.String => "tok-string",
            GuidedSqlTokenKind.Number => "tok-number",
            GuidedSqlTokenKind.Identifier => "tok-identifier",
            _ => null,
        };
    }
}
