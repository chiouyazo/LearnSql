using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace LearnSql.Sql.Dialect.Functions;

public static class FormatCultures
{
    private const string NarrowNoBreakSpace = " ";
    private const string NoBreakSpace = " ";

    private static readonly CultureDefinition EnglishUs = new CultureDefinition(
        "en-US",
        ".",
        ",",
        "$",
        0,
        0,
        1,
        "/",
        "M/d/yyyy",
        "dddd, MMMM d, yyyy",
        "h:mm tt",
        "h:mm:ss tt",
        "MMMM d",
        "MMMM yyyy",
        [
            "January",
            "February",
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December",
        ],
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
        ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"]
    );

    private static readonly CultureDefinition EnglishGb = EnglishUs with
    {
        Name = "en-GB",
        CurrencySymbol = "£",
        CurrencyNegativePattern = 1,
        PercentPositivePattern = 1,
        ShortDatePattern = "dd/MM/yyyy",
        LongDatePattern = "dddd, d MMMM yyyy",
        ShortTimePattern = "HH:mm",
        LongTimePattern = "HH:mm:ss",
        MonthDayPattern = "d MMMM",
        AbbreviatedMonthNames =
        [
            "Jan",
            "Feb",
            "Mar",
            "Apr",
            "May",
            "Jun",
            "Jul",
            "Aug",
            "Sept",
            "Oct",
            "Nov",
            "Dec",
        ],
    };

    private static readonly CultureDefinition German = new CultureDefinition(
        "de-DE",
        ",",
        ".",
        "€",
        3,
        8,
        0,
        ".",
        "dd.MM.yyyy",
        "dddd, d. MMMM yyyy",
        "HH:mm",
        "HH:mm:ss",
        "d. MMMM",
        "MMMM yyyy",
        [
            "Januar",
            "Februar",
            "März",
            "April",
            "Mai",
            "Juni",
            "Juli",
            "August",
            "September",
            "Oktober",
            "November",
            "Dezember",
        ],
        ["Jan", "Feb", "Mär", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dez"],
        ["Sonntag", "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag"],
        ["So", "Mo", "Di", "Mi", "Do", "Fr", "Sa"]
    );

    private static readonly CultureDefinition French = new CultureDefinition(
        "fr-FR",
        ",",
        NarrowNoBreakSpace,
        "€",
        3,
        8,
        0,
        "/",
        "dd/MM/yyyy",
        "dddd d MMMM yyyy",
        "HH:mm",
        "HH:mm:ss",
        "d MMMM",
        "MMMM yyyy",
        [
            "janvier",
            "février",
            "mars",
            "avril",
            "mai",
            "juin",
            "juillet",
            "août",
            "septembre",
            "octobre",
            "novembre",
            "décembre",
        ],
        [
            "janv.",
            "févr.",
            "mars",
            "avr.",
            "mai",
            "juin",
            "juil.",
            "août",
            "sept.",
            "oct.",
            "nov.",
            "déc.",
        ],
        ["dimanche", "lundi", "mardi", "mercredi", "jeudi", "vendredi", "samedi"],
        ["dim.", "lun.", "mar.", "mer.", "jeu.", "ven.", "sam."]
    );

    private static readonly CultureDefinition Spanish = new CultureDefinition(
        "es-ES",
        ",",
        ".",
        "€",
        3,
        8,
        0,
        "/",
        "d/M/yyyy",
        "dddd, d 'de' MMMM 'de' yyyy",
        "H:mm",
        "H:mm:ss",
        "d 'de' MMMM",
        "MMMM 'de' yyyy",
        [
            "enero",
            "febrero",
            "marzo",
            "abril",
            "mayo",
            "junio",
            "julio",
            "agosto",
            "septiembre",
            "octubre",
            "noviembre",
            "diciembre",
        ],
        ["ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic"],
        ["domingo", "lunes", "martes", "miércoles", "jueves", "viernes", "sábado"],
        ["dom", "lun", "mar", "mié", "jue", "vie", "sáb"]
    );

    private static readonly CultureDefinition Italian = new CultureDefinition(
        "it-IT",
        ",",
        ".",
        "€",
        3,
        8,
        1,
        "/",
        "dd/MM/yyyy",
        "dddd d MMMM yyyy",
        "HH:mm",
        "HH:mm:ss",
        "d MMMM",
        "MMMM yyyy",
        [
            "gennaio",
            "febbraio",
            "marzo",
            "aprile",
            "maggio",
            "giugno",
            "luglio",
            "agosto",
            "settembre",
            "ottobre",
            "novembre",
            "dicembre",
        ],
        ["gen", "feb", "mar", "apr", "mag", "giu", "lug", "ago", "set", "ott", "nov", "dic"],
        ["domenica", "lunedì", "martedì", "mercoledì", "giovedì", "venerdì", "sabato"],
        ["dom", "lun", "mar", "mer", "gio", "ven", "sab"]
    );

    private static readonly Lazy<FrozenDictionary<string, CultureInfo>> Cultures = new Lazy<
        FrozenDictionary<string, CultureInfo>
    >(Build);

    public static IReadOnlyList<string> SupportedNames =>
        [.. Definitions().Select((CultureDefinition definition) => definition.Name)];

    public static bool TryGet(string name, [NotNullWhen(true)] out CultureInfo? culture)
    {
        string trimmed = name.Trim();
        if (trimmed.Length == 0)
        {
            culture = CultureInfo.InvariantCulture;
            return true;
        }

        return Cultures.Value.TryGetValue(trimmed, out culture);
    }

    public static CultureInfo Default => Cultures.Value["en-US"];

    private static FrozenDictionary<string, CultureInfo> Build()
    {
        Dictionary<string, CultureInfo> result = new Dictionary<string, CultureInfo>(
            StringComparer.OrdinalIgnoreCase
        );
        foreach (CultureDefinition definition in Definitions())
        {
            CultureInfo culture = definition.CreateCultureInfo();
            result[definition.Name] = culture;
            string neutral = definition.Name.Substring(0, 2);
            result.TryAdd(neutral, culture);
        }

        return result.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<CultureDefinition> Definitions()
    {
        yield return EnglishUs;
        yield return EnglishGb;
        yield return German;
        yield return French;
        yield return Spanish;
        yield return Italian;
    }
}
