using System;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Interfaces;

/// <summary>
/// Contract shared by every appliance that can be handled by the repair center.
/// It only describes WHAT an appliance must offer, not HOW it is done.
/// </summary>
public interface IRepairable
{
    /// <summary>Unique serial number that identifies the appliance.</summary>
    string SerialNumber { get; }

    /// <summary>Current position of the appliance in the repair lifecycle.</summary>
    RepairStatus Status { get; }

    /// <summary>Calculates the total repair cost according to the appliance type.</summary>
    decimal CalculateRepairCost();

    /// <summary>Moves the appliance into the "under repair" state.</summary>
    void StartRepair();

    /// <summary>Marks the repair as finished on the given date.</summary>
    void CompleteRepair(DateTime completionDate);

    /// <summary>Hands the repaired appliance back to the customer.</summary>
    void DeliverToCustomer();

    /// <summary>Returns a readable, multi-line description of the appliance.</summary>
    string GetDetails();
}
