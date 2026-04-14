using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskManager.Api.Options;

namespace TaskManager.Api.Infrastructure.Messaging;

public class TaskEventsConsumer : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<TaskEventsConsumer> _logger;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public TaskEventsConsumer(
        IOptions<RabbitMqOptions> options,
        ILogger<TaskEventsConsumer> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var connection = CreateFactory().CreateConnection();
                using var channel = connection.CreateModel();

                EnsureTopology(channel);
                channel.BasicQos(0, 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (_, eventArgs) =>
                {
                    try
                    {
                        var payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                        var message = JsonSerializer.Deserialize<TaskEventMessage>(payload, _serializerOptions);

                        _logger.LogInformation(
                            "Evento processado da fila. Action={Action} TaskId={TaskId} UserId={UserId}",
                            message?.Action,
                            message?.TaskId,
                            message?.UserId);

                        channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Falha ao processar mensagem da fila {QueueName}", _options.QueueName);
                        channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    }
                };

                var consumerTag = channel.BasicConsume(_options.QueueName, autoAck: false, consumer);

                _logger.LogInformation("Consumidor RabbitMQ conectado na fila {QueueName}", _options.QueueName);

                while (!stoppingToken.IsCancellationRequested && connection.IsOpen)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }

                if (channel.IsOpen)
                {
                    channel.BasicCancel(consumerTag);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ indisponivel. Nova tentativa em 10 segundos.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
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
