using System;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Refrigerator: the cost depends on the compressor, the cooling gas
/// and an extra charge for large units.
/// </summary>
public sealed class Refrigerator : Appliance
{
    private const decimal LaborRatePerHour = 25.00m;
    private const decimal CompressorReplacementFee = 120.00m;
    private const decimal GasRefillFee = 35.00m;
    private const int LargeCapacityLiters = 500;
    private const decimal LargeCapacitySurcharge = 0.10m;

    private readonly int _capacityLiters;
    private bool _compressorReplaced;
    private bool _gasRefilled;

    public Refrigerator(
        string serialNumber,
        string brand,
        string model,
        string customerName,
        string problemDescription,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        int capacityLiters)
        : base(serialNumber, brand, model, customerName, problemDescription, receivedDate, expectedDeliveryDate)
    {
        if (!InputValidator.IsInRange(capacityLiters, 50, 1500))
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacityLiters),
                "Refrigerator capacity must be between 50 and 1500 liters.");
        }

        _capacityLiters = capacityLiters;
    }

    public override string ApplianceType => "Refrigerator";

    protected override decimal HourlyRate => LaborRatePerHour;

    /// <summary>Capacity is a physical property, so it is read-only.</summary>
    public int CapacityLiters => _capacityLiters;

    public bool CompressorReplaced
    {
        get => _compressorReplaced;
        set
        {
            EnsureEditable();
            _compressorReplaced = value;
        }
    }

    public bool GasRefilled
    {
        get => _gasRefilled;
        set
        {
            EnsureEditable();
            _gasRefilled = value;
        }
    }

    public override decimal CalculateRepairCost()
    {
        decimal total = BaseCost;

        if (_compressorReplaced)
        {
            total += CompressorReplacementFee;
        }

        if (_gasRefilled)
        {
            total += GasRefillFee;
        }

        if (_capacityLiters >= LargeCapacityLiters)
        {
            total = ApplySurcharge(total, LargeCapacitySurcharge);
        }

        return Math.Round(total, 2);
    }

    public override string GetDetails()
        => base.GetDetails() + Environment.NewLine
           + $"  Capacity: {_capacityLiters} L"
           + $" | Compressor replaced: {DisplayFormatter.YesNo(_compressorReplaced)}"
           + $" | Gas refilled: {DisplayFormatter.YesNo(_gasRefilled)}";
}
