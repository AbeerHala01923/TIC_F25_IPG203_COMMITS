using System;
using System.Linq;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Interfaces;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Abstract base class of every appliance handled by the center.
/// It implements <see cref="IRepairable"/>, stores the data that all appliances share
/// and leaves the cost calculation to the concrete appliance types.
/// </summary>
public abstract class Appliance : IRepairable
{
    /// <summary>Fixed fee charged for inspecting any appliance.</summary>
    protected const decimal DiagnosticFee = 15.00m;

    // ----- Private fields (encapsulation) -----
    // Fields marked readonly can only be assigned inside the constructor.
    private readonly string _serialNumber;
    private readonly string _brand;
    private readonly string _model;
    private readonly string _customerName;
    private readonly string _problemDescription;
    private readonly DateTime _receivedDate;

    private DateTime _expectedDeliveryDate;
    private DateTime? _completionDate;
    private RepairStatus _status;
    private double _laborHours;
    private decimal _partsCost;

    // ----- Events (delegates) -----
    public event ApplianceStatusChangedHandler? StatusChanged;
    public event DeliveryDelayedHandler? DeliveryDelayed;

    protected Appliance(
        string serialNumber,
        string brand,
        string model,
        string customerName,
        string problemDescription,
        DateTime receivedDate,
        DateTime expectedDeliveryDate)
    {
        _serialNumber = InputValidator.RequireSerialNumber(serialNumber);
        _brand = InputValidator.RequireText(brand, nameof(brand));
        _model = InputValidator.RequireText(model, nameof(model), 1);
        _customerName = InputValidator.RequireText(customerName, nameof(customerName), 3);
        _problemDescription = InputValidator.RequireText(problemDescription, nameof(problemDescription), 5);
        InputValidator.RequireDeliveryWindow(receivedDate, expectedDeliveryDate);

        _receivedDate = receivedDate.Date;
        _expectedDeliveryDate = expectedDeliveryDate.Date;
        _status = RepairStatus.Received;
    }

    // ----- Read-only properties: fixed once the object is created -----
    public string SerialNumber => _serialNumber;
    public string Brand => _brand;
    public string Model => _model;
    public string CustomerName => _customerName;
    public string ProblemDescription => _problemDescription;
    public DateTime ReceivedDate => _receivedDate;

    // ----- Read-only from outside: changed only through the lifecycle methods -----
    public RepairStatus Status => _status;
    public DateTime ExpectedDeliveryDate => _expectedDeliveryDate;
    public DateTime? CompletionDate => _completionDate;

    /// <summary>True once the repair work is done (completed or delivered).</summary>
    public bool IsFinished => _status == RepairStatus.Completed || _status == RepairStatus.Delivered;

    // ----- Properties with validation, editable only while the repair is open -----
    public double LaborHours
    {
        get => _laborHours;
        set
        {
            EnsureEditable();
            if (!InputValidator.IsValidLaborHours(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"Labor hours must be between 0 and {InputValidator.MaxLaborHours}.");
            }

            _laborHours = value;
        }
    }

    public decimal PartsCost
    {
        get => _partsCost;
        set
        {
            EnsureEditable();
            if (!InputValidator.IsValidAmount(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Parts cost must be a non-negative amount.");
            }

            _partsCost = value;
        }
    }

    // ----- Abstract members: every appliance type must supply its own version -----

    /// <summary>Human-readable name of the appliance type (for example "Refrigerator").</summary>
    public abstract string ApplianceType { get; }

    /// <summary>Hourly labor rate, which differs from one appliance type to another.</summary>
    protected abstract decimal HourlyRate { get; }

    /// <summary>
    /// Calculates the full repair cost. Each subclass applies the rules of its own appliance type.
    /// </summary>
    public abstract decimal CalculateRepairCost();

    // ----- Shared cost helpers used by the subclasses -----
    protected decimal LaborCost => Math.Round((decimal)_laborHours * HourlyRate, 2);

    protected decimal BaseCost => DiagnosticFee + LaborCost + _partsCost;

    protected static decimal ApplySurcharge(decimal amount, decimal rate)
        => Math.Round(amount * (1 + rate), 2);

    // ----- Lifecycle operations -----
    public void Diagnose()
        => MoveTo(RepairStatus.Diagnosing, RepairStatus.Received);

    public void StartRepair()
        => MoveTo(RepairStatus.UnderRepair, RepairStatus.Received, RepairStatus.Diagnosing);

    public void CompleteRepair(DateTime completionDate)
    {
        EnsureTransitionAllowed(RepairStatus.Completed, RepairStatus.UnderRepair);

        if (!InputValidator.IsValidDate(completionDate) || completionDate.Date < _receivedDate)
        {
            throw new ArgumentException(
                "Completion date must be a valid date on or after the received date.",
                nameof(completionDate));
        }

        _completionDate = completionDate.Date;
        ChangeStatus(RepairStatus.Completed);
    }

    public void DeliverToCustomer()
        => MoveTo(RepairStatus.Delivered, RepairStatus.Completed);

    /// <summary>Moves the promised delivery date, for example after a part arrives late.</summary>
    public void RescheduleDelivery(DateTime newExpectedDeliveryDate)
    {
        if (_status == RepairStatus.Delivered)
        {
            throw new InvalidOperationException("A delivered appliance cannot be rescheduled.");
        }

        InputValidator.RequireDeliveryWindow(_receivedDate, newExpectedDeliveryDate);
        _expectedDeliveryDate = newExpectedDeliveryDate.Date;
    }

    /// <summary>
    /// Raises <see cref="DeliveryDelayed"/> when the appliance is still with the center
    /// after the promised delivery date. Returns true if the appliance is late.
    /// </summary>
    public bool CheckDelay(DateTime currentDate)
    {
        if (_status == RepairStatus.Delivered || currentDate.Date <= _expectedDeliveryDate)
        {
            return false;
        }

        int daysLate = (currentDate.Date - _expectedDeliveryDate).Days;
        OnDeliveryDelayed(daysLate);
        return true;
    }

    /// <summary>Readable description; subclasses extend it with their own details.</summary>
    public virtual string GetDetails()
    {
        string completion = _completionDate.HasValue
            ? DisplayFormatter.FormatDate(_completionDate.Value)
            : "-";

        return string.Join(
            Environment.NewLine,
            $"[{ApplianceType}] {Brand} {Model} | S/N: {SerialNumber} | Status: {Status}",
            $"  Customer: {CustomerName} | Problem: {ProblemDescription}",
            $"  Received: {DisplayFormatter.FormatDate(_receivedDate)}"
                + $" | Expected delivery: {DisplayFormatter.FormatDate(_expectedDeliveryDate)}"
                + $" | Completed: {completion}",
            $"  Labor: {DisplayFormatter.FormatHours(_laborHours)}"
                + $" | Parts: {DisplayFormatter.FormatMoney(_partsCost)}"
                + $" | Repair cost: {DisplayFormatter.FormatMoney(CalculateRepairCost())}");
    }

    public override string ToString()
        => $"{ApplianceType} {Brand} {Model} ({SerialNumber})";

    // ----- Protected helpers for subclasses -----

    /// <summary>Blocks changes to repair data once the repair has been completed.</summary>
    protected void EnsureEditable()
    {
        if (IsFinished)
        {
            throw new InvalidOperationException(
                $"Appliance {_serialNumber} is already {_status}; its repair data can no longer be changed.");
        }
    }

    // ----- Event raising -----
    protected virtual void OnStatusChanged(RepairStatus oldStatus, RepairStatus newStatus)
        => StatusChanged?.Invoke(this, new ApplianceStatusChangedEventArgs(this, oldStatus, newStatus));

    protected virtual void OnDeliveryDelayed(int daysLate)
        => DeliveryDelayed?.Invoke(this, new DeliveryDelayedEventArgs(this, daysLate));

    // ----- Private status machinery -----
    private void MoveTo(RepairStatus newStatus, params RepairStatus[] allowedFrom)
    {
        EnsureTransitionAllowed(newStatus, allowedFrom);
        ChangeStatus(newStatus);
    }

    private void EnsureTransitionAllowed(RepairStatus newStatus, params RepairStatus[] allowedFrom)
    {
        if (!allowedFrom.Contains(_status))
        {
            throw new InvalidOperationException(
                $"Appliance {_serialNumber} cannot move from {_status} to {newStatus}.");
        }
    }

    private void ChangeStatus(RepairStatus newStatus)
    {
        RepairStatus oldStatus = _status;
        _status = newStatus;
        OnStatusChanged(oldStatus, newStatus);
    }
}
