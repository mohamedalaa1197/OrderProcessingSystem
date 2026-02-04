namespace OrderProcessing.Shared.Events;

public class OrderCreatedEvent
{
    public Order Order { get; set; }
}