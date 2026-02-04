using RabbitMQ.Client;

namespace OrderProcessing.Infrastructure;

public class RabbitMQConnectionFactory
{
    private readonly RabbitMQConfiguration _config;
    private IConnection _connection;
    private readonly Lock _lock = new Lock();

    public RabbitMQConnectionFactory(RabbitMQConfiguration config)
    {
        _config = config;
    }

    public IConnection GetConnection()
    {
        lock (_lock)
        {
            if (_connection is not { IsOpen: true })
            {
                var factory = new ConnectionFactory
                {
                    HostName = _config.HostName,
                    Port = _config.Port,
                    UserName = _config.UserName,
                    Password = _config.Password,
                    VirtualHost = _config.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnection();
            }
        }


        return _connection;
    }
}