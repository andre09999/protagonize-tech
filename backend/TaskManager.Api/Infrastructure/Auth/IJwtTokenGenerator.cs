using TaskManager.Api.Entities;

namespace TaskManager.Api.Infrastructure.Auth;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(AppUser user);
}
