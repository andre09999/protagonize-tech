using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TaskManager.Api.Options;

namespace TaskManager.Api.Infrastructure.Messaging;

public class RabbitMqTaskEventPublisher : ITaskEventPublisher
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqTaskEventPublisher> _logger;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public RabbitMqTaskEventPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqTaskEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task PublishAsync(TaskEventMessage message, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        try
        {
            using var connection = CreateFactory().CreateConnection();
            using var channel = connection.CreateModel();

            EnsureTopology(channel);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, _serializerOptions));
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Evento da tarefa publicado com sucesso. Action={Action} TaskId={TaskId} UserId={UserId}",
                message.Action,
                message.TaskId,
                message.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao publicar evento da tarefa {TaskId} no RabbitMQ", message.TaskId);
        }

        return Task.CompletedTask;
    }

    private ConnectionFactory CreateFactory() =>
        new()
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            DispatchConsumersAsync = false,
            AutomaticRecoveryEnabled = true
        };

    private void EnsureTopology(IModel channel)
    {
        channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
        channel.QueueDeclare(_options.QueueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(_options.QueueName, _options.ExchangeName, _options.RoutingKey);
    }
}
