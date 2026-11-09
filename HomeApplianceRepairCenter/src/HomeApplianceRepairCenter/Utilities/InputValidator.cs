using System;
using System.Text.RegularExpressions;

namespace HomeApplianceRepairCenter.Utilities;

/// <summary>
/// Static class that validates every value entering the system.
/// It keeps no state, so no instance is ever needed.
/// "Is..." methods answer true/false; "Require..." methods throw when the value is invalid
/// and return the cleaned value when it is valid.
/// </summary>
public static class InputValidator
{
    /// <summary>Largest number of labor hours accepted for a single repair.</summary>
    public const double MaxLaborHours = 100;

    private const decimal MaxAmount = 1_000_000m;
    private const int MinYear = 2000;
    private const int MaxYear = 2100;

    // Serial number format: 2-3 uppercase letters, a hyphen, then exactly 6 digits (e.g. RF-100234).
    private static readonly Regex SerialNumberPattern =
        new(@"^[A-Z]{2,3}-\d{6}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // ----- Boolean checks -----

    public static bool IsValidSerialNumber(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && SerialNumberPattern.IsMatch(value.Trim().ToUpperInvariant());

    public static bool IsValidText(string? value, int minLength = 2)
        => !string.IsNullOrWhiteSpace(value) && value.Trim().Length >= minLength;

    public static bool IsValidDate(DateTime date)
        => date.Year >= MinYear && date.Year <= MaxYear;

    /// <summary>The delivery date must be a valid date that is not before the received date.</summary>
    public static bool IsValidDeliveryWindow(DateTime receivedDate, DateTime expectedDeliveryDate)
        => IsValidDate(receivedDate)
           && IsValidDate(expectedDeliveryDate)
           && expectedDeliveryDate.Date >= receivedDate.Date;

    public static bool IsValidLaborHours(double hours)
        => !double.IsNaN(hours) && hours >= 0 && hours <= MaxLaborHours;

    public static bool IsValidAmount(decimal amount)
        => amount >= 0 && amount <= MaxAmount;

    public static bool IsInRange(int value, int min, int max)
        => value >= min && value <= max;

    // ----- Checks that throw -----

    public static string RequireSerialNumber(string? value, string paramName = "serialNumber")
    {
        if (!IsValidSerialNumber(value))
        {
            throw new ArgumentException(
                "Serial number must be 2-3 uppercase letters, a hyphen and 6 digits (for example RF-100234).",
                paramName);
        }

        return value!.Trim().ToUpperInvariant();
    }

    public static string RequireText(string? value, string paramName, int minLength = 2)
    {
        if (!IsValidText(value, minLength))
        {
            throw new ArgumentException(
                $"Value must contain at least {minLength} non-blank character(s).",
                paramName);
        }

        return value!.Trim();
    }

    public static void RequireDeliveryWindow(DateTime receivedDate, DateTime expectedDeliveryDate)
    {
        if (!IsValidDeliveryWindow(receivedDate, expectedDeliveryDate))
        {
            throw new ArgumentException(
                "Expected delivery date must be a valid date on or after the received date.",
                nameof(expectedDeliveryDate));
        }
    }
}
