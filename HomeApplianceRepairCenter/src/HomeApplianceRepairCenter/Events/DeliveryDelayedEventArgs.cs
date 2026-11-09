using System;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Events;

/// <summary>
/// Data sent to subscribers when an appliance has not been delivered
/// by the promised date.
/// </summary>
public class DeliveryDelayedEventArgs : EventArgs
{
    public DeliveryDelayedEventArgs(Appliance appliance, int daysLate)
    {
        Appliance = appliance ?? throw new ArgumentNullException(nameof(appliance));
        DaysLate = daysLate;
    }

    /// <summary>The late appliance.</summary>
    public Appliance Appliance { get; }

    /// <summary>Number of days past the expected delivery date.</summary>
    public int DaysLate { get; }

    /// <summary>The delivery date that was promised to the customer.</summary>
    public DateTime ExpectedDeliveryDate => Appliance.ExpectedDeliveryDate;
}
