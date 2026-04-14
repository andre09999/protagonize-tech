using TaskManager.Api.Contracts.Tasks;
using TaskManager.Api.Entities;
using TaskManager.Api.Infrastructure.Auth;
using TaskManager.Api.Infrastructure.Caching;
using TaskManager.Api.Infrastructure.CurrentUser;
using TaskManager.Api.Infrastructure.Messaging;

namespace TaskManager.Api.Tests;

internal sealed class FakeCurrentUserService : ICurrentUserService
{
    public int UserId { get; set; } = 1;
    public string Username { get; set; } = "tester";
}

internal sealed class FakeTaskCacheService : ITaskCacheService
{
    private readonly Dictionary<string, IReadOnlyCollection<TaskResponse>> _taskLists = new();
    private readonly Dictionary<string, TaskResponse> _tasks = new();

    public List<string> InvalidatedKeys { get; } = new();

    public Task<IReadOnlyCollection<TaskResponse>?> GetTasksAsync(int userId, string? status, CancellationToken cancellationToken = default)
    {
        _taskLists.TryGetValue(GetListKey(userId, status), out var tasks);
        return Task.FromResult(tasks);
    }

    public Task SetTasksAsync(int userId, string? status, IReadOnlyCollection<TaskResponse> tasks, CancellationToken cancellationToken = default)
    {
        _taskLists[GetListKey(userId, status)] = tasks;
        return Task.CompletedTask;
    }

    public Task<TaskResponse?> GetTaskAsync(int userId, int taskId, CancellationToken cancellationToken = default)
    {
        _tasks.TryGetValue(GetTaskKey(userId, taskId), out var task);
        return Task.FromResult(task);
    }

    public Task SetTaskAsync(int userId, TaskResponse task, CancellationToken cancellationToken = default)
    {
        _tasks[GetTaskKey(userId, task.Id)] = task;
        return Task.CompletedTask;
    }

    public Task InvalidateTaskAsync(int userId, int taskId, CancellationToken cancellationToken = default)
    {
        var key = GetTaskKey(userId, taskId);
        _tasks.Remove(key);
        InvalidatedKeys.Add(key);
        return Task.CompletedTask;
    }

    public Task InvalidateTaskListsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var prefix = $"tasks:user:{userId}:list:";

        foreach (var key in _taskLists.Keys.Where(key => key.StartsWith(prefix)).ToList())
        {
            _taskLists.Remove(key);
        }

        InvalidatedKeys.Add(prefix);
        return Task.CompletedTask;
    }

    public void SeedTaskList(int userId, string? status, IReadOnlyCollection<TaskResponse> tasks) =>
        _taskLists[GetListKey(userId, status)] = tasks;

    private static string GetListKey(int userId, string? status) => $"tasks:user:{userId}:list:{status ?? "all"}";

    private static string GetTaskKey(int userId, int taskId) => $"tasks:user:{userId}:item:{taskId}";
}

internal sealed class FakeTaskEventPublisher : ITaskEventPublisher
{
    public List<TaskEventMessage> PublishedMessages { get; } = new();

    public Task PublishAsync(TaskEventMessage message, CancellationToken cancellationToken = default)
    {
        PublishedMessages.Add(message);
        return Task.CompletedTask;
    }
}

internal sealed class StubJwtTokenGenerator : IJwtTokenGenerator
{
    public (string Token, DateTime ExpiresAtUtc) GenerateToken(AppUser user) =>
        ("fake-jwt-token", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc));
}
