using System;
using System.Collections.Generic;
using System.Linq;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Models;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Services;

/// <summary>
/// The main class of the system. It owns a list of <see cref="Appliance"/> objects
/// (refrigerators, washing machines, air conditioners...) and works with them
/// only through the base type, relying on polymorphism.
/// It also listens to every registered appliance and republishes its events,
/// so the rest of the application needs to subscribe in one place only.
/// </summary>
public class RepairCenter
{
    private readonly string _name;
    private readonly List<Appliance> _appliances = new();

    /// <summary>Raised whenever any registered appliance changes status.</summary>
    public event ApplianceStatusChangedHandler? ApplianceStatusChanged;

    /// <summary>Raised when a registered appliance is late for delivery.</summary>
    public event DeliveryDelayedHandler? DeliveryDelayed;

    public RepairCenter(string name)
    {
        _name = InputValidator.RequireText(name, nameof(name), 3);
    }

    public string Name => _name;

    public int ApplianceCount => _appliances.Count;

    public IReadOnlyList<Appliance> Appliances => _appliances.AsReadOnly();

    /// <summary>Adds an appliance to the center and starts listening to its events.</summary>
    public void RegisterAppliance(Appliance appliance)
    {
        if (appliance is null)
        {
            throw new ArgumentNullException(nameof(appliance));
        }

        if (FindBySerialNumber(appliance.SerialNumber) is not null)
        {
            throw new InvalidOperationException(
                $"An appliance with serial number {appliance.SerialNumber} is already registered.");
        }

        appliance.StatusChanged += HandleStatusChanged;
        appliance.DeliveryDelayed += HandleDeliveryDelayed;

        _appliances.Add(appliance);
        RepairStatistics.Track(appliance);
    }

    public Appliance? FindBySerialNumber(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return null;
        }

        string normalized = serialNumber.Trim().ToUpperInvariant();
        return _appliances.FirstOrDefault(a => a.SerialNumber == normalized);
    }

    public IReadOnlyList<Appliance> GetAppliancesByStatus(RepairStatus status)
        => _appliances.Where(a => a.Status == status).ToList();

    public IReadOnlyList<T> GetAppliancesByType<T>() where T : Appliance
        => _appliances.OfType<T>().ToList();

    /// <summary>
    /// Sum of the repair costs of all appliances. Each call to CalculateRepairCost()
    /// runs the version that belongs to the real type of the appliance.
    /// </summary>
    public decimal CalculateTotalRepairCost()
        => _appliances.Sum(a => a.CalculateRepairCost());

    /// <summary>Sum of the repair costs of the appliances whose repair is finished.</summary>
    public decimal CalculateCollectedRevenue()
        => _appliances.Where(a => a.IsFinished).Sum(a => a.CalculateRepairCost());

    /// <summary>Total repair cost grouped by appliance type.</summary>
    public IReadOnlyDictionary<string, decimal> GetCostSummaryByType()
        => _appliances
            .GroupBy(a => a.ApplianceType)
            .ToDictionary(group => group.Key, group => group.Sum(a => a.CalculateRepairCost()));

    /// <summary>Description of every appliance, built through the polymorphic GetDetails() method.</summary>
    public string GetStatusReport()
    {
        if (_appliances.Count == 0)
        {
            return "No appliances are registered.";
        }

        var descriptions = new List<string>();
        foreach (Appliance appliance in _appliances)
        {
            descriptions.Add(appliance.GetDetails());
        }

        return string.Join(Environment.NewLine + Environment.NewLine, descriptions);
    }

    /// <summary>
    /// Asks every appliance whether it is late on the given date.
    /// Each late appliance raises its own event. Returns the number of late appliances.
    /// </summary>
    public int CheckDeliveryDelays(DateTime currentDate)
    {
        int delayedCount = 0;
        foreach (Appliance appliance in _appliances)
        {
            if (appliance.CheckDelay(currentDate))
            {
                delayedCount++;
            }
        }

        return delayedCount;
    }

    // ----- Event forwarding -----
    private void HandleStatusChanged(object? sender, ApplianceStatusChangedEventArgs e)
        => ApplianceStatusChanged?.Invoke(this, e);

    private void HandleDeliveryDelayed(object? sender, DeliveryDelayedEventArgs e)
        => DeliveryDelayed?.Invoke(this, e);
}
