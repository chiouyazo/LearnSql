namespace LearnSql.Core.Services.Navigation;

public sealed record NavigationEntry(
    string Href,
    string TitleKey,
    string Icon,
    int Order,
    NavLinkMatch Match = NavLinkMatch.Prefix
);
