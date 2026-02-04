namespace OrderProcessing.Shared.Events;

public class ShippingPreparedEvent
{
    public Guid OrderId { get; set; }
    public string TrackingNumber { get; set; }
}