using System.Text.Json;
using OrderProcessing.Infrastructure;
using OrderProcessing.Shared.Events;

namespace OrderProcessing.PaymentWorker;

public class PaymentProcessingWorker : RabbitMQConsumer
{
    private readonly RabbitMqPublisher _publisher;
    protected override string QueueName => QueueNames.PaymentProcessing;
    protected override string ExchangeName => ExchangeNames.OrderEvents;
    protected override string RoutingKey => RoutingKeys.OrderCreated;
    protected override string DeadLetterQueue => QueueNames.PaymentDLQ;

    public PaymentProcessingWorker(
        RabbitMQConnectionFactory connectionFactory,
        RabbitMqPublisher publisher)
        : base(connectionFactory)
    {
        _publisher = publisher;
    }

    protected override async Task ProcessMessageAsync(string message)
    {
        var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);

        Console.WriteLine($"Processing payment for Order: {orderEvent.Order.OrderId}");

        // Simulate payment processing
        // here we just do the payment logic actually
        await Task.Delay(2000);

        // Simulate 90% success rate
        // not every time successful
        var success = Random.Shared.Next(100) < 90;

        var paymentEvent = new PaymentProcessedEvent
        {
            OrderId = orderEvent.Order.OrderId,
            Success = success,
            TransactionId = success ? Guid.NewGuid().ToString() : null
        };

        if (success)
        {
            Console.WriteLine($"Payment successful for Order: {orderEvent.Order.OrderId}");
            _publisher.Publish(
                exchangeName: ExchangeNames.PaymentEvents,
                routingKey: RoutingKeys.PaymentSuccess,
                message: paymentEvent);
        }
        else
        {
            Console.WriteLine($"Payment failed for Order: {orderEvent.Order.OrderId}");
            _publisher.Publish(
                exchangeName: ExchangeNames.PaymentEvents,
                routingKey: RoutingKeys.PaymentFailed,
                message: paymentEvent
            );
        }
    }
}