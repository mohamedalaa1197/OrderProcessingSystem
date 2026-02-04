using System.Text.Json;
using OrderProcessing.Infrastructure;
using OrderProcessing.Shared.Events;

namespace OrderProcessing.InventoryWorker;

public class InventoryReservationWorker : RabbitMQConsumer
{
    private readonly RabbitMqPublisher _publisher;
    protected override string QueueName => QueueNames.InventoryReservation;
    protected override string ExchangeName => ExchangeNames.PaymentEvents;
    protected override string RoutingKey => RoutingKeys.PaymentSuccess;
    protected override string DeadLetterQueue => QueueNames.InventoryDLQ;

    public InventoryReservationWorker(
        RabbitMQConnectionFactory connectionFactory,
        RabbitMqPublisher publisher) : base(connectionFactory)
    {
        _publisher = publisher;
    }


    protected override async Task ProcessMessageAsync(string message)
    {
        // recieve the payment processed event
        var paymentEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(message);

        Console.WriteLine($"Reserving inventory for Order: {paymentEvent.OrderId}");

        // doint the logic of reserving inventory
        // here we just simulate it with a delay
        await Task.Delay(1500);

        var success = Random.Shared.Next(100) < 95;

        var inventoryEvent = new InventoryReservedEvent
        {
            OrderId = paymentEvent.OrderId,
            Success = success,
            FailureReason = success ? null : "Item out of stock"
        };

        if (success)
        {
            Console.WriteLine($"Inventory reserved for Order: {paymentEvent.OrderId}");
            _publisher.Publish(
                exchangeName: ExchangeNames.InventoryEvents,
                routingKey: RoutingKeys.InventoryReserved,
                message: inventoryEvent);
        }
        else
        {
            Console.WriteLine($"Inventory reservation failed for Order: {paymentEvent.OrderId}");
            _publisher.Publish(
                exchangeName: ExchangeNames.InventoryEvents,
                routingKey: RoutingKeys.InventoryFailed,
                message: inventoryEvent);
        }
    }
}