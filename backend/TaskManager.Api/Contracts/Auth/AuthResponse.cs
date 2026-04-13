namespace TaskManager.Api.Contracts.Auth;

public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, AuthUserResponse User);
