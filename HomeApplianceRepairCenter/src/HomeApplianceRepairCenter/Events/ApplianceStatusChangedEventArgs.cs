using System;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Events;

/// <summary>
/// Data sent to subscribers when the status of an appliance changes.
/// The object is immutable so a subscriber cannot alter what others receive.
/// </summary>
public class ApplianceStatusChangedEventArgs : EventArgs
{
    public ApplianceStatusChangedEventArgs(Appliance appliance, RepairStatus oldStatus, RepairStatus newStatus)
    {
        Appliance = appliance ?? throw new ArgumentNullException(nameof(appliance));
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }

    /// <summary>The appliance whose status changed.</summary>
    public Appliance Appliance { get; }

    /// <summary>Status before the change.</summary>
    public RepairStatus OldStatus { get; }

    /// <summary>Status after the change.</summary>
    public RepairStatus NewStatus { get; }
}
