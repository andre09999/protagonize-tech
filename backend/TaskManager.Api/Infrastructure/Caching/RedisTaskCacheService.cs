using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using TaskManager.Api.Common.Models;
using TaskManager.Api.Contracts.Tasks;
using TaskManager.Api.Options;

namespace TaskManager.Api.Infrastructure.Caching;

public class RedisTaskCacheService : ITaskCacheService
{
    private readonly IDistributedCache _cache;
    private readonly RedisOptions _options;
    private readonly ILogger<RedisTaskCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public RedisTaskCacheService(
        IDistributedCache cache,
        IOptions<RedisOptions> options,
        ILogger<RedisTaskCacheService> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<TaskResponse>?> GetTasksAsync(int userId, string? status, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await _cache.GetStringAsync(GetListKey(userId, status), cancellationToken);

            if (string.IsNullOrWhiteSpace(payload))
            {
                return null;
            }

            return JsonSerializer.Deserialize<List<TaskResponse>>(payload, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao ler cache de tarefas do usuario {UserId}", userId);
            return null;
        }
    }

    public async Task SetTasksAsync(int userId, string? status, IReadOnlyCollection<TaskResponse> tasks, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.SetStringAsync(
                GetListKey(userId, status),
                JsonSerializer.Serialize(tasks, _jsonOptions),
                CreateEntryOptions(),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao gravar cache da lista de tarefas do usuario {UserId}", userId);
        }
    }

    public async Task<TaskResponse?> GetTaskAsync(int userId, int taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await _cache.GetStringAsync(GetTaskKey(userId, taskId), cancellationToken);

            if (string.IsNullOrWhiteSpace(payload))
            {
                return null;
            }

            return JsonSerializer.Deserialize<TaskResponse>(payload, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao ler cache da tarefa {TaskId} do usuario {UserId}", taskId, userId);
            return null;
        }
    }

    public async Task SetTaskAsync(int userId, TaskResponse task, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.SetStringAsync(
                GetTaskKey(userId, task.Id),
                JsonSerializer.Serialize(task, _jsonOptions),
                CreateEntryOptions(),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao gravar cache da tarefa {TaskId} do usuario {UserId}", task.Id, userId);
        }
    }

    public async Task InvalidateTaskAsync(int userId, int taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(GetTaskKey(userId, taskId), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao invalidar cache da tarefa {TaskId} do usuario {UserId}", taskId, userId);
        }
    }

    public async Task InvalidateTaskListsAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(GetListKey(userId, null), cancellationToken);

            foreach (var status in TaskStatuses.All)
            {
                await _cache.RemoveAsync(GetListKey(userId, status), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao invalidar cache das listas de tarefas do usuario {UserId}", userId);
        }
    }

    private DistributedCacheEntryOptions CreateEntryOptions() =>
        new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.DefaultExpirationMinutes)
        };

    private static string GetListKey(int userId, string? status)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? "all"
            : TaskStatuses.Normalize(status).ToLowerInvariant();

        return $"tasks:user:{userId}:list:{normalizedStatus}";
    }

    private static string GetTaskKey(int userId, int taskId) => $"tasks:user:{userId}:item:{taskId}";
}
