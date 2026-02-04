using System.Text.Json;
using OrderProcessing.Infrastructure;
using OrderProcessing.Shared.Events;

namespace OrderProcessing.NotificationWorker;

public class NotificationWorker : RabbitMQConsumer
{
    public NotificationWorker(RabbitMQConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    protected override string QueueName => QueueNames.Notification;
    protected override string ExchangeName => ExchangeNames.ShippingEvents;
    protected override string RoutingKey => RoutingKeys.ShippingPrepared;
    protected override string DeadLetterQueue => "";

    protected override Task ProcessMessageAsync(string message)
    {
        var shippingPreparedEvent = JsonSerializer.Deserialize<ShippingPreparedEvent>(message);
        Console.WriteLine($"Sending notification for Order: {shippingPreparedEvent.OrderId}, Tracking Number: {shippingPreparedEvent.TrackingNumber}");

        // Simulate sending notification
        Task.Delay(1000);

        Console.WriteLine("Notification sent.");
        return Task.CompletedTask;
    }
}