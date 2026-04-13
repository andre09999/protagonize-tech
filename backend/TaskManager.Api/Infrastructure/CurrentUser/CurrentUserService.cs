using System.Security.Claims;

namespace TaskManager.Api.Infrastructure.CurrentUser;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var userIdClaim = GetClaimValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Usuario autenticado invalido.");
            }

            return userId;
        }
    }

    public string Username => GetClaimValue(ClaimTypes.Name);

    private string GetClaimValue(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new UnauthorizedAccessException("Nao foi possivel identificar o usuario autenticado.");
        }

        return value;
    }
}
