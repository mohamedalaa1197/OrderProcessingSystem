using System.Text.Json;
using OrderProcessing.Infrastructure;
using OrderProcessing.Shared.Events;

namespace OrderProcessing.ShippingWorker;

public class ShippingPreparingWorker : RabbitMQConsumer
{
    private readonly RabbitMQPublisher _publisher;
    protected override string QueueName => QueueNames.ShippingPreparation;

    protected override string DeadLetterQueue => QueueNames.ShippingDLQ;

    // prepare
    public ShippingPreparingWorker(
        RabbitMQConnectionFactory connectionFactory,
        RabbitMQPublisher publisher) : base(connectionFactory)
    {
        _publisher = publisher;
    }


    protected override async Task ProcessMessageAsync(string message)
    {
        var inventoryEvent = JsonSerializer.Deserialize<InventoryReservedEvent>(message);
        Console.WriteLine($"Preparing shipping for Order: {inventoryEvent.OrderId}");


        // doint the logic of reserving inventory
        // here we just simulate it with a delay
        await Task.Delay(1500);

        var success = Random.Shared.Next(100) < 95;

        var notificationEvent = new ShippingPreparedEvent()
        {
            OrderId = inventoryEvent.OrderId,
            TrackingNumber = Guid.NewGuid().ToString(),
        };

        if (success)
        {
            Console.WriteLine($"Inventory reserved for Order: {notificationEvent.OrderId}");
            _publisher.Publish(QueueNames.Notification, notificationEvent);
        }
        else
        {
            Console.WriteLine($"Inventory reservation failed for Order: {notificationEvent.OrderId}");
        }
    }
}