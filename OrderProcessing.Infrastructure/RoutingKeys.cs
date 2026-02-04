namespace OrderProcessing.Infrastructure;

public class RoutingKeys
{
    // Order routing keys
    public const string OrderCreated = "order.created";

    // Payment routing keys
    public const string PaymentProcessing = "payment.processing";
    public const string PaymentSuccess = "payment.success";
    public const string PaymentFailed = "payment.failed";

    // Inventory routing keys
    public const string InventoryReserve = "inventory.reserve";
    public const string InventoryReserved = "inventory.reserved";
    public const string InventoryFailed = "inventory.failed";

    // Shipping routing keys
    public const string ShippingPrepare = "shipping.prepare";
    public const string ShippingPrepared = "shipping.prepared";

    // Notification routing keys
    public const string NotifyCustomer = "notification.customer";
}