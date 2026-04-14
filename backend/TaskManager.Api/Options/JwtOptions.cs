namespace TaskManager.Api.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "TaskManager.Api";
    public string Audience { get; set; } = "TaskManager.Web";
    public string Key { get; set; } = "change-this-super-secret-key-with-at-least-32-characters";
    public int ExpirationMinutes { get; set; } = 120;
}
