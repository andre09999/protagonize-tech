namespace TaskManager.Api.Contracts.Auth;

public sealed record RegisterRequest
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
