namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Lifecycle of an appliance inside the repair center.
/// A normal repair moves forward through these states in order.
/// </summary>
public enum RepairStatus
{
    /// <summary>The appliance was handed in by the customer.</summary>
    Received,

    /// <summary>A technician is inspecting the appliance to find the fault.</summary>
    Diagnosing,

    /// <summary>The repair work is in progress.</summary>
    UnderRepair,

    /// <summary>The repair is finished and the appliance waits for pickup.</summary>
    Completed,

    /// <summary>The appliance was handed back to the customer.</summary>
    Delivered
}
