namespace OrderProcessing.Infrastructure;

public static class QueueNames
{
    public const string OrderCreated = "order.created";
    public const string PaymentProcessing = "payment.processing";
    public const string InventoryReservation = "inventory.reservation";
    public const string ShippingPreparation = "shipping.preparation";
    public const string Notification = "notification";

    // Dead letter queues
    public const string PaymentDLQ = "payment.processing.dlq";
    public const string InventoryDLQ = "inventory.reservation.dlq";
    public const string ShippingDLQ = "shipping.preparation.dlq";
    public const string NotificationDLQ = "notification.reservation.dlq";
}