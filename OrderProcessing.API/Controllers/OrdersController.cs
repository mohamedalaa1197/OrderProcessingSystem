using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Infrastructure;
using OrderProcessing.Shared;
using OrderProcessing.Shared.Events;

namespace OrderProcessing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly RabbitMqPublisher _publisher;

    public OrdersController(RabbitMqPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = request.CustomerId.ToString(),
            Items = request.Items,
            TotalAmount = request.Items.Sum(i => i.Price * i.Quantity),
            ShippingAddress = request.ShippingAddress,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var orderEvent = new OrderCreatedEvent { Order = order };

        _publisher.Publish(
            exchangeName: ExchangeNames.OrderEvents,
            routingKey: QueueNames.OrderCreated,
            message: orderEvent);

        return Ok(new
        {
            orderId = order.OrderId,
            message = "Order created successfully"
        });
    }
}

public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public string ShippingAddress { get; set; }
}