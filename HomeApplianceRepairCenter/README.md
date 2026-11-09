# Home Appliance Repair Center

A C# console application that manages a home appliance repair center
(refrigerators, washing machines and air conditioners) and demonstrates the core
principles of object-oriented programming.

## Requirements

- .NET SDK 8.0 or newer

## Run

```bash
dotnet run --project src/HomeApplianceRepairCenter
```

Or open `HomeApplianceRepairCenter.sln` in Visual Studio 2022 and press F5.

## Project structure

```text
HomeApplianceRepairCenter.sln
src/HomeApplianceRepairCenter/
├── Interfaces/
│   └── IRepairable.cs                  Interface shared by all appliances
├── Models/
│   ├── Appliance.cs                    Abstract base class
│   ├── Refrigerator.cs                 Subclass
│   ├── WashingMachine.cs               Subclass
│   ├── AirConditioner.cs               Subclass
│   └── RepairStatus.cs                 Repair lifecycle states
├── Events/
│   ├── RepairEventHandlers.cs          Delegates
│   ├── ApplianceStatusChangedEventArgs.cs
│   └── DeliveryDelayedEventArgs.cs
├── Services/
│   └── RepairCenter.cs                 Main class holding List<Appliance>
├── Utilities/
│   ├── InputValidator.cs               Static validation class
│   ├── RepairStatistics.cs             Static class with static properties
│   └── DisplayFormatter.cs             Shared text formatting
└── Program.cs                          Demonstration scenario
```

## OOP principles and where they live

| Principle | Where |
|-----------|-------|
| Abstraction | `IRepairable`, `Appliance`, abstract `CalculateRepairCost()` |
| Inheritance | `Refrigerator`, `WashingMachine`, `AirConditioner` extend `Appliance` |
| Polymorphism | Overridden `CalculateRepairCost()` and `GetDetails()`; `RepairCenter` works on `List<Appliance>` |
| Encapsulation | Private fields, validated properties, read-only `SerialNumber`, `ReceivedDate`... |
| Delegates and events | `ApplianceStatusChangedHandler`, `DeliveryDelayedHandler`, `StatusChanged`, `DeliveryDelayed` |
| Static members | `InputValidator` (static class), `RepairStatistics` (static properties) |
