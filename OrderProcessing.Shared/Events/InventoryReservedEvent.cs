namespace OrderProcessing.Shared.Events;

public class InventoryReservedEvent
{
    public Guid OrderId { get; set; }
    public bool Success { get; set; }
    public string FailureReason { get; set; }
}