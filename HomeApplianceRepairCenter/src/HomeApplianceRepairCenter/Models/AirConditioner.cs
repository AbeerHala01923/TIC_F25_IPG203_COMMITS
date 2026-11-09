using System;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Air conditioner: the cost depends on the refrigerant gas, the inverter board,
/// the difficulty of reaching the outdoor unit and an extra charge for high-capacity units.
/// </summary>
public sealed class AirConditioner : Appliance
{
    private const decimal LaborRatePerHour = 30.00m;
    private const decimal GasRefillFee = 55.00m;
    private const decimal InverterBoardFee = 90.00m;
    private const decimal HighAccessFee = 40.00m;
    private const int HighCapacityBtu = 24000;
    private const decimal HighCapacitySurcharge = 0.08m;

    private readonly int _coolingCapacityBtu;
    private readonly bool _isInverter;
    private bool _gasRefilled;
    private bool _inverterBoardReplaced;
    private bool _outdoorUnitHighMounted;

    public AirConditioner(
        string serialNumber,
        string brand,
        string model,
        string customerName,
        string problemDescription,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        int coolingCapacityBtu,
        bool isInverter)
        : base(serialNumber, brand, model, customerName, problemDescription, receivedDate, expectedDeliveryDate)
    {
        if (!InputValidator.IsInRange(coolingCapacityBtu, 6000, 60000))
        {
            throw new ArgumentOutOfRangeException(
                nameof(coolingCapacityBtu),
                "Cooling capacity must be between 6000 and 60000 BTU.");
        }

        _coolingCapacityBtu = coolingCapacityBtu;
        _isInverter = isInverter;
    }

    public override string ApplianceType => "Air Conditioner";

    protected override decimal HourlyRate => LaborRatePerHour;

    /// <summary>Capacity and compressor type are physical properties, so they are read-only.</summary>
    public int CoolingCapacityBtu => _coolingCapacityBtu;

    public bool IsInverter => _isInverter;

    public bool GasRefilled
    {
        get => _gasRefilled;
        set
        {
            EnsureEditable();
            _gasRefilled = value;
        }
    }

    /// <summary>Only inverter units have an inverter board that can be replaced.</summary>
    public bool InverterBoardReplaced
    {
        get => _inverterBoardReplaced;
        set
        {
            EnsureEditable();
            if (value && !_isInverter)
            {
                throw new InvalidOperationException("Only an inverter air conditioner has an inverter board.");
            }

            _inverterBoardReplaced = value;
        }
    }

    /// <summary>True when the outdoor unit needs special equipment to be reached.</summary>
    public bool OutdoorUnitHighMounted
    {
        get => _outdoorUnitHighMounted;
        set
        {
            EnsureEditable();
            _outdoorUnitHighMounted = value;
        }
    }

    public override decimal CalculateRepairCost()
    {
        decimal total = BaseCost;

        if (_gasRefilled)
        {
            total += GasRefillFee;
        }

        if (_inverterBoardReplaced)
        {
            total += InverterBoardFee;
        }

        if (_outdoorUnitHighMounted)
        {
            total += HighAccessFee;
        }

        if (_coolingCapacityBtu >= HighCapacityBtu)
        {
            total = ApplySurcharge(total, HighCapacitySurcharge);
        }

        return Math.Round(total, 2);
    }

    public override string GetDetails()
        => base.GetDetails() + Environment.NewLine
           + $"  Capacity: {_coolingCapacityBtu} BTU"
           + $" | Inverter: {DisplayFormatter.YesNo(_isInverter)}"
           + $" | Gas refilled: {DisplayFormatter.YesNo(_gasRefilled)}"
           + $" | Inverter board replaced: {DisplayFormatter.YesNo(_inverterBoardReplaced)}"
           + $" | High-mounted outdoor unit: {DisplayFormatter.YesNo(_outdoorUnitHighMounted)}";
}
