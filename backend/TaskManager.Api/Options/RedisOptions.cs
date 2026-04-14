namespace TaskManager.Api.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";
    public int DefaultExpirationMinutes { get; set; } = 10;
}
