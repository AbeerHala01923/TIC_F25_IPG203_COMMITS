using System.Collections.Generic;
using System.Linq;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Utilities;

/// <summary>
/// Static class that exposes center-wide figures (counts and totals) through static properties.
/// The figures are always computed from the current state of the tracked appliances,
/// so they can never become out of date.
/// </summary>
public static class RepairStatistics
{
    private static readonly List<Appliance> _trackedAppliances = new();

    /// <summary>Number of appliances registered in the center.</summary>
    public static int TotalTracked => _trackedAppliances.Count;

    /// <summary>Appliances that are still waiting for or undergoing repair.</summary>
    public static int InProgressCount
        => _trackedAppliances.Count(a => a.Status is RepairStatus.Received
                                         or RepairStatus.Diagnosing
                                         or RepairStatus.UnderRepair);

    /// <summary>Appliances whose repair is finished (completed or already delivered).</summary>
    public static int RepairedCount
        => _trackedAppliances.Count(a => a.IsFinished);

    /// <summary>Appliances that were handed back to their owners.</summary>
    public static int DeliveredCount
        => CountByStatus(RepairStatus.Delivered);

    /// <summary>Total cost of all finished repairs.</summary>
    public static decimal TotalRevenue
        => _trackedAppliances.Where(a => a.IsFinished).Sum(a => a.CalculateRepairCost());

    /// <summary>Estimated cost of the repairs that are still in progress.</summary>
    public static decimal PendingEstimatedRevenue
        => _trackedAppliances.Where(a => !a.IsFinished).Sum(a => a.CalculateRepairCost());

    /// <summary>Counts the tracked appliances that are in the given status.</summary>
    public static int CountByStatus(RepairStatus status)
        => _trackedAppliances.Count(a => a.Status == status);

    /// <summary>Starts tracking an appliance. Called by the repair center when an appliance is registered.</summary>
    internal static void Track(Appliance appliance)
    {
        if (!_trackedAppliances.Contains(appliance))
        {
            _trackedAppliances.Add(appliance);
        }
    }

    /// <summary>Stops tracking every appliance (useful when a fresh start is needed).</summary>
    public static void Reset() => _trackedAppliances.Clear();
}
