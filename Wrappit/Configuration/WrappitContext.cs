using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Wrappit.Configuration;

internal class WrappitContext : IWrappitContext
{
    private static readonly object LockObject = new();

    private readonly IWrappitOptions _options;
    private readonly ILogger<WrappitContext> _logger;
    
    private IConnection? _connection;
    
    public IConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                lock (LockObject)
                {
                    if (_connection == null)
                    {
                        _connection = _options.CreateFactory().CreateConnection();
                        using var channel = _connection.CreateModel();
                        channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, true, false, null);
                        
                        _logger.LogDebug("Created exchange with name {exchangeName}.", _options.ExchangeName);
                    }
                }
            }

            return _connection;
        }
    }

    public string ExchangeName => _options.ExchangeName;
    public string QueueName => _options.QueueName;
    public string QueueType => _options.QueueType;
    public int DeliveryLimit => _options.DeliveryLimit;

    public WrappitContext(IWrappitOptions options, ILogger<WrappitContext> logger)
    {
        _options = options;
        _logger = logger;
    }
    
    public IModel CreateChannel()
    {
        return Connection.CreateModel();
    }
    
    public void Dispose()
    {
        _connection?.Dispose();
    }
}
