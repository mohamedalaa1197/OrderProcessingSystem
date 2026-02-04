using System.Text.Json;
using OrderProcessing.Infrastructure;
using OrderProcessing.Shared.Events;

namespace OrderProcessing.ShippingWorker;

public class ShippingPreparingWorker : RabbitMQConsumer
{
    private readonly RabbitMqPublisher _publisher;
    protected override string QueueName => QueueNames.ShippingPreparation;
    protected override string ExchangeName => ExchangeNames.InventoryEvents;
    protected override string RoutingKey => RoutingKeys.InventoryReserved;
    protected override string DeadLetterQueue => QueueNames.ShippingDLQ;

    // prepare
    public ShippingPreparingWorker(
        RabbitMQConnectionFactory connectionFactory,
        RabbitMqPublisher publisher) : base(connectionFactory)
    {
        _publisher = publisher;
    }


    protected override async Task ProcessMessageAsync(string message)
    {
        Console.WriteLine($"Received message for shipping preparation: {message} from Queue: {QueueName}");
        var inventoryEvent = JsonSerializer.Deserialize<InventoryReservedEvent>(message);
        Console.WriteLine($"Preparing shipping for Order: {inventoryEvent.OrderId}");


        // doint the logic of reserving inventory
        // here we just simulate it with a delay
        await Task.Delay(1500);

        var shippingEvent = new ShippingPreparedEvent()
        {
            OrderId = inventoryEvent.OrderId,
            TrackingNumber = Guid.NewGuid().ToString(),
        };


        Console.WriteLine($"Inventory reserved for Order: {shippingEvent.OrderId}");
        _publisher.Publish(
            exchangeName: ExchangeNames.ShippingEvents,
            routingKey: RoutingKeys.ShippingPrepared,
            message: shippingEvent);
    }
}