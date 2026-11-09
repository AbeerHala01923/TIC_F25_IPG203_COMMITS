using System;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Washing machine: the cost depends on the drum bearing, the water pump,
/// the loading type and an extra charge for heavy-duty machines.
/// </summary>
public sealed class WashingMachine : Appliance
{
    private const decimal LaborRatePerHour = 20.00m;
    private const decimal DrumBearingFee = 85.00m;
    private const decimal PumpReplacementFee = 45.00m;
    private const decimal FrontLoadHandlingFee = 12.00m;
    private const int HeavyDutyKg = 10;
    private const decimal HeavyDutySurcharge = 0.05m;

    private readonly int _loadCapacityKg;
    private readonly bool _isFrontLoad;
    private bool _drumBearingReplaced;
    private bool _pumpReplaced;

    public WashingMachine(
        string serialNumber,
        string brand,
        string model,
        string customerName,
        string problemDescription,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        int loadCapacityKg,
        bool isFrontLoad)
        : base(serialNumber, brand, model, customerName, problemDescription, receivedDate, expectedDeliveryDate)
    {
        if (!InputValidator.IsInRange(loadCapacityKg, 3, 25))
        {
            throw new ArgumentOutOfRangeException(
                nameof(loadCapacityKg),
                "Washing machine capacity must be between 3 and 25 kg.");
        }

        _loadCapacityKg = loadCapacityKg;
        _isFrontLoad = isFrontLoad;
    }

    public override string ApplianceType => "Washing Machine";

    protected override decimal HourlyRate => LaborRatePerHour;

    /// <summary>Capacity and loading type are physical properties, so they are read-only.</summary>
    public int LoadCapacityKg => _loadCapacityKg;

    public bool IsFrontLoad => _isFrontLoad;

    public bool DrumBearingReplaced
    {
        get => _drumBearingReplaced;
        set
        {
            EnsureEditable();
            _drumBearingReplaced = value;
        }
    }

    public bool PumpReplaced
    {
        get => _pumpReplaced;
        set
        {
            EnsureEditable();
            _pumpReplaced = value;
        }
    }

    public override decimal CalculateRepairCost()
    {
        decimal total = BaseCost;

        if (_drumBearingReplaced)
        {
            total += DrumBearingFee;
        }

        if (_pumpReplaced)
        {
            total += PumpReplacementFee;
        }

        if (_isFrontLoad)
        {
            total += FrontLoadHandlingFee;
        }

        if (_loadCapacityKg >= HeavyDutyKg)
        {
            total = ApplySurcharge(total, HeavyDutySurcharge);
        }

        return Math.Round(total, 2);
    }

    public override string GetDetails()
        => base.GetDetails() + Environment.NewLine
           + $"  Capacity: {_loadCapacityKg} kg"
           + $" | Front load: {DisplayFormatter.YesNo(_isFrontLoad)}"
           + $" | Drum bearing replaced: {DisplayFormatter.YesNo(_drumBearingReplaced)}"
           + $" | Pump replaced: {DisplayFormatter.YesNo(_pumpReplaced)}";
}
