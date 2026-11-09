using System;
using System.Collections.Generic;
using System.Text;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Models;
using HomeApplianceRepairCenter.Services;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter;

/// <summary>
/// Console demonstration of the repair center. The scenario registers several appliances,
/// moves them through the repair lifecycle, reacts to events and prints the statistics.
/// </summary>
public static class Program
{
    // The "current" date of the scenario is fixed so the output is the same on every run.
    private static readonly DateTime Today = new(2026, 11, 8);

    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var center = new RepairCenter("Damascus Home Appliance Repair Center");

        // Subscribe once to the center; it forwards the events of all appliances.
        center.ApplianceStatusChanged += OnApplianceStatusChanged;
        center.DeliveryDelayed += OnDeliveryDelayed;

        PrintHeader("1. Registering appliances");
        RegisterSampleAppliances(center);
        Console.WriteLine($"  {center.ApplianceCount} appliances registered in {center.Name}.");

        PrintHeader("2. Working on the repairs (events are printed as they happen)");
        ProcessRepairs(center);

        PrintHeader("3. Checking delivery delays on " + DisplayFormatter.FormatDate(Today));
        int delayed = center.CheckDeliveryDelays(Today);
        Console.WriteLine($"  Late appliances: {delayed}");

        PrintHeader("4. Rescheduling a late appliance and checking again");
        Appliance lateAirConditioner = GetAppliance<AirConditioner>(center, "AC-300455");
        lateAirConditioner.RescheduleDelivery(new DateTime(2026, 11, 12));
        delayed = center.CheckDeliveryDelays(Today);
        Console.WriteLine($"  Late appliances after rescheduling: {delayed}");

        PrintHeader("5. Status report (polymorphic GetDetails)");
        Console.WriteLine(center.GetStatusReport());

        PrintHeader("6. Costs (polymorphic CalculateRepairCost)");
        Console.WriteLine($"  Total of all repair costs : {DisplayFormatter.FormatMoney(center.CalculateTotalRepairCost())}");
        Console.WriteLine($"  Collected (finished) only  : {DisplayFormatter.FormatMoney(center.CalculateCollectedRevenue())}");
        foreach (KeyValuePair<string, decimal> entry in center.GetCostSummaryByType())
        {
            Console.WriteLine($"  {entry.Key,-16}: {DisplayFormatter.FormatMoney(entry.Value)}");
        }

        PrintHeader("7. Static statistics (RepairStatistics)");
        PrintStatistics();

        PrintHeader("8. Validation and protection of data");
        DemonstrateValidation(center);
    }

    private static void RegisterSampleAppliances(RepairCenter center)
    {
        center.RegisterAppliance(new Refrigerator(
            "RF-100234", "Samsung", "RT50", "Ahmad Khaled", "Not cooling, compressor noise",
            new DateTime(2026, 10, 20), new DateTime(2026, 10, 27), 550));

        center.RegisterAppliance(new WashingMachine(
            "WM-200451", "LG", "F4V5", "Rana Haddad", "Drum makes a loud grinding noise",
            new DateTime(2026, 10, 25), new DateTime(2026, 11, 5), 9, true));

        center.RegisterAppliance(new AirConditioner(
            "AC-300871", "Gree", "Pular 18", "Omar Saleh", "Weak cooling, low gas pressure",
            new DateTime(2026, 11, 1), new DateTime(2026, 11, 12), 18000, true));

        center.RegisterAppliance(new Refrigerator(
            "RF-100877", "Bosch", "KGN36", "Layla Nasser", "Door seal leaking cold air",
            new DateTime(2026, 11, 3), new DateTime(2026, 11, 15), 380));

        center.RegisterAppliance(new WashingMachine(
            "WM-200912", "Ariston", "AVL 70", "Sami Youssef", "Machine does not drain water",
            new DateTime(2026, 11, 4), new DateTime(2026, 11, 10), 7, false));

        center.RegisterAppliance(new AirConditioner(
            "AC-300455", "Carrier", "Optimax 24", "Nour Ibrahim", "Outdoor unit stops after a few minutes",
            new DateTime(2026, 10, 28), new DateTime(2026, 11, 6), 24000, false));
    }

    private static void ProcessRepairs(RepairCenter center)
    {
        // Refrigerator RF-100234: full cycle from diagnosis to delivery.
        var fridge = GetAppliance<Refrigerator>(center, "RF-100234");
        fridge.Diagnose();
        fridge.LaborHours = 3.5;
        fridge.PartsCost = 60m;
        fridge.CompressorReplaced = true;
        fridge.GasRefilled = true;
        fridge.StartRepair();
        fridge.CompleteRepair(new DateTime(2026, 10, 26));
        fridge.DeliverToCustomer();

        // Washing machine WM-200451: repaired but the customer has not collected it yet.
        var washer = GetAppliance<WashingMachine>(center, "WM-200451");
        washer.StartRepair();
        washer.LaborHours = 2;
        washer.PartsCost = 35m;
        washer.DrumBearingReplaced = true;
        washer.CompleteRepair(new DateTime(2026, 11, 4));

        // Air conditioner AC-300871: repair still in progress.
        var inverterUnit = GetAppliance<AirConditioner>(center, "AC-300871");
        inverterUnit.Diagnose();
        inverterUnit.LaborHours = 2.5;
        inverterUnit.PartsCost = 120m;
        inverterUnit.GasRefilled = true;
        inverterUnit.StartRepair();

        // Air conditioner AC-300455: repair in progress and already past its delivery date.
        var largeUnit = GetAppliance<AirConditioner>(center, "AC-300455");
        largeUnit.StartRepair();
        largeUnit.LaborHours = 4;
        largeUnit.OutdoorUnitHighMounted = true;

        // Washing machine WM-200912: only diagnosed so far.
        GetAppliance<WashingMachine>(center, "WM-200912").Diagnose();

        // Refrigerator RF-100877 stays in the "Received" state on purpose.
    }

    private static T GetAppliance<T>(RepairCenter center, string serialNumber) where T : Appliance
        => center.FindBySerialNumber(serialNumber) as T
           ?? throw new InvalidOperationException($"Appliance {serialNumber} was not found.");

    // ----- Event handlers (methods that match the delegate signatures) -----
    private static void OnApplianceStatusChanged(object? sender, ApplianceStatusChangedEventArgs e)
    {
        Appliance appliance = e.Appliance;
        Console.WriteLine($"  [EVENT] {appliance.ApplianceType} {appliance.SerialNumber}: {e.OldStatus} -> {e.NewStatus}");

        if (e.NewStatus == RepairStatus.Completed)
        {
            Console.WriteLine(
                $"          >> Repair completed. Tell {appliance.CustomerName} the appliance is ready"
                + $" (cost: {DisplayFormatter.FormatMoney(appliance.CalculateRepairCost())}).");
        }
    }

    private static void OnDeliveryDelayed(object? sender, DeliveryDelayedEventArgs e)
    {
        Appliance appliance = e.Appliance;
        Console.WriteLine(
            $"  [ALERT] {appliance.ApplianceType} {appliance.SerialNumber} is {e.DaysLate} day(s) late"
            + $" (promised {DisplayFormatter.FormatDate(e.ExpectedDeliveryDate)}). Contact {appliance.CustomerName}.");
    }

    private static void PrintStatistics()
    {
        Console.WriteLine($"  Registered appliances        : {RepairStatistics.TotalTracked}");
        Console.WriteLine($"  Still in progress            : {RepairStatistics.InProgressCount}");
        Console.WriteLine($"  Repaired (completed/delivered): {RepairStatistics.RepairedCount}");
        Console.WriteLine($"  Delivered to customers       : {RepairStatistics.DeliveredCount}");
        Console.WriteLine($"  Total revenue (finished)     : {DisplayFormatter.FormatMoney(RepairStatistics.TotalRevenue)}");
        Console.WriteLine($"  Pending estimated revenue    : {DisplayFormatter.FormatMoney(RepairStatistics.PendingEstimatedRevenue)}");
        Console.WriteLine("  Count by status:");

        foreach (RepairStatus status in Enum.GetValues<RepairStatus>())
        {
            Console.WriteLine($"    {status,-12}: {RepairStatistics.CountByStatus(status)}");
        }
    }

    private static void DemonstrateValidation(RepairCenter center)
    {
        Console.WriteLine($"  IsValidSerialNumber(\"AC-300871\") = {InputValidator.IsValidSerialNumber("AC-300871")}");
        Console.WriteLine($"  IsValidSerialNumber(\"AC300871\")  = {InputValidator.IsValidSerialNumber("AC300871")}");
        Console.WriteLine();

        DemonstrateRejection(
            "Invalid serial number",
            () => new Refrigerator("BAD-1", "Samsung", "RT50", "Test Customer", "Not cooling", Today, Today.AddDays(5), 300));

        DemonstrateRejection(
            "Delivery date before received date",
            () => new WashingMachine("WM-200999", "LG", "F2J3", "Test Customer", "Does not spin", Today, Today.AddDays(-2), 8, true));

        DemonstrateRejection(
            "Capacity outside the allowed range",
            () => new AirConditioner("AC-300999", "Gree", "Test", "Test Customer", "Does not start", Today, Today.AddDays(4), 500, false));

        DemonstrateRejection(
            "Duplicate serial number",
            () => center.RegisterAppliance(new Refrigerator(
                "RF-100234", "Samsung", "RT50", "Test Customer", "Not cooling", Today, Today.AddDays(5), 300)));

        var receivedFridge = GetAppliance<Refrigerator>(center, "RF-100877");
        DemonstrateRejection(
            "Completing a repair that never started",
            () => receivedFridge.CompleteRepair(Today));

        var deliveredFridge = GetAppliance<Refrigerator>(center, "RF-100234");
        DemonstrateRejection(
            "Editing the parts cost of a delivered appliance",
            () => deliveredFridge.PartsCost = 10m);

        var nonInverter = GetAppliance<AirConditioner>(center, "AC-300455");
        DemonstrateRejection(
            "Inverter board on a non-inverter unit",
            () => nonInverter.InverterBoardReplaced = true);
    }

    private static void DemonstrateRejection(string description, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"  [UNEXPECTED] {description}: no error was raised.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"  [REJECTED] {description}");
            Console.WriteLine($"             {ex.Message.Replace(Environment.NewLine, " ")}");
        }
    }

    private static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 78));
        Console.WriteLine(title);
        Console.WriteLine(new string('=', 78));
    }
}
