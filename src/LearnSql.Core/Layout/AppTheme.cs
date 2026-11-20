namespace LearnSql.Core.Layout;

public static class AppTheme
{
    private static readonly string[] FontFamily =
    [
        "system-ui",
        "-apple-system",
        "Segoe UI",
        "Roboto",
        "Helvetica Neue",
        "Arial",
        "sans-serif",
    ];

    public static MudTheme Create()
    {
        return new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = "#594ae2",
                Black = "#110e2d",
                AppbarText = "#424242",
                AppbarBackground = "rgba(255,255,255,0.8)",
                DrawerBackground = "#ffffff",
                GrayLight = "#e8e8e8",
                GrayLighter = "#f9f9f9",
            },
            PaletteDark = new PaletteDark()
            {
                Primary = "#7e6fff",
                Surface = "#1e1e2d",
                Background = "#1a1a27",
                BackgroundGray = "#151521",
                AppbarText = "#b2b0bf",
                AppbarBackground = "rgba(26,26,39,0.8)",
                DrawerBackground = "#1a1a27",
                ActionDefault = "#74718e",
                ActionDisabled = "#9999994d",
                ActionDisabledBackground = "#605f6d4d",
                TextPrimary = "#b2b0bf",
                TextSecondary = "#92929f",
                TextDisabled = "#ffffff33",
                DrawerIcon = "#92929f",
                DrawerText = "#92929f",
                GrayLight = "#2a2833",
                GrayLighter = "#1e1e2d",
                Info = "#4a86ff",
                Success = "#3dcb6c",
                Warning = "#ffb545",
                Error = "#ff3f5f",
                LinesDefault = "#33323e",
                TableLines = "#33323e",
                Divider = "#292838",
                OverlayLight = "#1e1e2d80",
            },
            Typography = new Typography()
            {
                Default = new DefaultTypography() { FontFamily = FontFamily },
            },
            LayoutProperties = new LayoutProperties(),
        };
    }
}
