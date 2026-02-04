namespace OrderProcessing.Shared;

public class Order
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderItem
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public enum OrderStatus
{
    Pending,
    PaymentProcessing,
    PaymentConfirmed,
    PaymentFailed,
    InventoryReserved,
    InventoryFailed,
    ReadyForShipping,
    Shipped,
    Completed,
    Cancelled
}