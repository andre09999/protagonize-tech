using TaskManager.Api.Contracts.Tasks;

namespace TaskManager.Api.Infrastructure.Caching;

public interface ITaskCacheService
{
    Task<IReadOnlyCollection<TaskResponse>?> GetTasksAsync(int userId, string? status, CancellationToken cancellationToken = default);
    Task SetTasksAsync(int userId, string? status, IReadOnlyCollection<TaskResponse> tasks, CancellationToken cancellationToken = default);
    Task<TaskResponse?> GetTaskAsync(int userId, int taskId, CancellationToken cancellationToken = default);
    Task SetTaskAsync(int userId, TaskResponse task, CancellationToken cancellationToken = default);
    Task InvalidateTaskAsync(int userId, int taskId, CancellationToken cancellationToken = default);
    Task InvalidateTaskListsAsync(int userId, CancellationToken cancellationToken = default);
}
