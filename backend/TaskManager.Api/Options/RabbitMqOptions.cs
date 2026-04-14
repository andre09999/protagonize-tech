namespace TaskManager.Api.Options;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "taskmanager.exchange";
    public string QueueName { get; set; } = "taskmanager.tasks";
    public string RoutingKey { get; set; } = "task.changed";
}
