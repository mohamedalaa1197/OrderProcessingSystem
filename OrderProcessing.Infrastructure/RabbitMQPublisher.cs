using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace OrderProcessing.Infrastructure;

public class RabbitMqPublisher : IDisposable
{
    private readonly IModel _channel;

    public RabbitMqPublisher(RabbitMQConnectionFactory rabbitMqConnectionFactory)
    {
        var connection = rabbitMqConnectionFactory.GetConnection();
        _channel = connection.CreateModel();

        // declaring all the exchanges
        DeclareExchanges();
    }

    public void Publish<T>(string exchangeName, string routingKey, T message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.DeliveryMode = 2;
        properties.ContentType = "application/json";
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        _channel.BasicPublish(
            exchange: exchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body
        );

        Console.WriteLine($"Published to exchange: {exchangeName}, routing key: {routingKey}");
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
    }

    private void DeclareExchanges()
    {
        // Declare direct exchanges for each service domain
        _channel.ExchangeDeclare(
            exchange: ExchangeNames.OrderEvents,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        _channel.ExchangeDeclare(
            exchange: ExchangeNames.PaymentEvents,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        _channel.ExchangeDeclare(
            exchange: ExchangeNames.InventoryEvents,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        _channel.ExchangeDeclare(
            exchange: ExchangeNames.ShippingEvents,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        _channel.ExchangeDeclare(
            exchange: ExchangeNames.NotificationEvents,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );
    }
}