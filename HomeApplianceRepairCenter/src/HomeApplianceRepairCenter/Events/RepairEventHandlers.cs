namespace HomeApplianceRepairCenter.Events;

/// <summary>
/// Delegate that describes the signature of any method interested in
/// status changes of an appliance (for example: repair completed).
/// </summary>
public delegate void ApplianceStatusChangedHandler(object? sender, ApplianceStatusChangedEventArgs e);

/// <summary>
/// Delegate that describes the signature of any method interested in
/// late deliveries.
/// </summary>
public delegate void DeliveryDelayedHandler(object? sender, DeliveryDelayedEventArgs e);
