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
    protected abstract string DeadLetterQueue { get; }

    protected RabbitMQConsumer(RabbitMQConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = _connectionFactory.GetConnection();
        _channel = _connection.CreateModel();

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


        var args = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(DeadLetterQueue))
        {
            args.Add("x-dead-letter-exchange", "");
            args.Add("x-dead-letter-routing-key", DeadLetterQueue);
        }

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args
        );

        // Set prefetch count (quality of service)
        // no limit on the message size
        // only process one message at a time
        // global is false, so it is not applying to this consumer only
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                await ProcessMessageAsync(message);

                // Acknowledge message
                _channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");

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