using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace OrderProcessing.Infrastructure;

public class RabbitMQPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQPublisher(RabbitMQConnectionFactory rabbitMqConnectionFactory)
    {
        _connection = rabbitMqConnectionFactory.GetConnection();
        _channel = _connection.CreateModel();
    }

    public void Publish<T>(string queueName, T message)
    {
        // Using the channel to declare a queue
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.DeliveryMode = 2;

        // Using the channel to publish a message
        _channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: properties,
            body: body
        );
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
    }
}