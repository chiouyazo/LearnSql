using System.Globalization;

namespace LearnSql.Sql.Dialect.Functions;

public sealed record CultureDefinition(
    string Name,
    string DecimalSeparator,
    string GroupSeparator,
    string CurrencySymbol,
    int CurrencyPositivePattern,
    int CurrencyNegativePattern,
    int PercentPositivePattern,
    string DateSeparator,
    string ShortDatePattern,
    string LongDatePattern,
    string ShortTimePattern,
    string LongTimePattern,
    string MonthDayPattern,
    string YearMonthPattern,
    IReadOnlyList<string> MonthNames,
    IReadOnlyList<string> AbbreviatedMonthNames,
    IReadOnlyList<string> DayNames,
    IReadOnlyList<string> AbbreviatedDayNames
)
{
    private const string AmDesignator = "AM";
    private const string PmDesignator = "PM";

    public CultureInfo CreateCultureInfo()
    {
        CultureInfo culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        ApplyNumberFormat(culture.NumberFormat);
        ApplyDateTimeFormat(culture.DateTimeFormat);
        return CultureInfo.ReadOnly(culture);
    }

    private void ApplyNumberFormat(NumberFormatInfo format)
    {
        format.NumberDecimalSeparator = DecimalSeparator;
        format.NumberGroupSeparator = GroupSeparator;
        format.CurrencyDecimalSeparator = DecimalSeparator;
        format.CurrencyGroupSeparator = GroupSeparator;
        format.PercentDecimalSeparator = DecimalSeparator;
        format.PercentGroupSeparator = GroupSeparator;
        format.CurrencySymbol = CurrencySymbol;
        format.CurrencyPositivePattern = CurrencyPositivePattern;
        format.CurrencyNegativePattern = CurrencyNegativePattern;
        format.PercentPositivePattern = PercentPositivePattern;
    }

    private void ApplyDateTimeFormat(DateTimeFormatInfo format)
    {
        string[] months = [.. MonthNames, string.Empty];
        string[] abbreviatedMonths = [.. AbbreviatedMonthNames, string.Empty];
        format.MonthNames = months;
        format.MonthGenitiveNames = months;
        format.AbbreviatedMonthNames = abbreviatedMonths;
        format.AbbreviatedMonthGenitiveNames = abbreviatedMonths;
        format.DayNames = [.. DayNames];
        format.AbbreviatedDayNames = [.. AbbreviatedDayNames];
        format.ShortestDayNames = [.. AbbreviatedDayNames.Select(name => name.Substring(0, 2))];
        format.AMDesignator = AmDesignator;
        format.PMDesignator = PmDesignator;
        format.DateSeparator = DateSeparator;
        format.ShortDatePattern = ShortDatePattern;
        format.LongDatePattern = LongDatePattern;
        format.ShortTimePattern = ShortTimePattern;
        format.LongTimePattern = LongTimePattern;
        format.MonthDayPattern = MonthDayPattern;
        format.YearMonthPattern = YearMonthPattern;
        format.FullDateTimePattern = LongDatePattern + " " + LongTimePattern;
    }
}
