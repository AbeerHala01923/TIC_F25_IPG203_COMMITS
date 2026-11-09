using System;
using System.Globalization;

namespace HomeApplianceRepairCenter.Utilities;

/// <summary>
/// Small helpers that turn values into text in one consistent format,
/// so the formatting rules are written only once.
/// </summary>
public static class DisplayFormatter
{
    public static string FormatMoney(decimal amount)
        => amount.ToString("N2", CultureInfo.InvariantCulture) + " USD";

    public static string FormatDate(DateTime date)
        => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string FormatHours(double hours)
        => hours.ToString("0.0", CultureInfo.InvariantCulture) + " h";

    public static string YesNo(bool value)
        => value ? "Yes" : "No";
}
