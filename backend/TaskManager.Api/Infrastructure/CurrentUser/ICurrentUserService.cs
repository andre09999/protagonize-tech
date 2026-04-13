namespace TaskManager.Api.Infrastructure.CurrentUser;

public interface ICurrentUserService
{
    int UserId { get; }
    string Username { get; }
}
