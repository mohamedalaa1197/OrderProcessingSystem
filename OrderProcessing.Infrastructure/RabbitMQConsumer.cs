using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderProcessing.Infrastructure;

public abstract class RabbitMQConsumer : BackgroundService
{
    private readonly RabbitMQConnectionFactory _connectionFactory;
    private IConnection _connection;
    private IModel _channel;
    protected abstract string QueueName { get; }
    protected abstract string ExchangeName { get; }
    protected abstract string RoutingKey { get; }
    protected abstract string DeadLetterQueue { get; }

    protected RabbitMQConsumer(RabbitMQConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = _connectionFactory.GetConnection();
        _channel = _connection.CreateModel();

        // Declare the exchange
        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        // Declare dead letter queue if specified
        if (!string.IsNullOrEmpty(DeadLetterQueue))
        {
            _channel.QueueDeclare(
                queue: DeadLetterQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        // Declare main queue with DLQ configuration
        var queueArgs = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(DeadLetterQueue))
        {
            queueArgs.Add("x-dead-letter-exchange", "");
            queueArgs.Add("x-dead-letter-routing-key", DeadLetterQueue);
        }

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs
        );

        // Bind queue to exchange with routing key
        _channel.QueueBind(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey
        );

        Console.WriteLine($"Queue '{QueueName}' bound to exchange '{ExchangeName}' with routing key '{RoutingKey}'");

        // Set prefetch count (quality of service)
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Received message on queue '{QueueName}': {message.Substring(0, Math.Min(100, message.Length))}...");

                await ProcessMessageAsync(message);

                // Acknowledge message
                _channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message on queue '{QueueName}': {ex.Message}");

                // Reject and send to DLQ
                _channel.BasicReject(deliveryTag: eventArgs.DeliveryTag, requeue: false);
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    protected abstract Task ProcessMessageAsync(string message);

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        base.Dispose();
    }
}