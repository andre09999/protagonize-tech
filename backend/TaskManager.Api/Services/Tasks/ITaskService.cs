using TaskManager.Api.Contracts.Tasks;

namespace TaskManager.Api.Services.Tasks;

public interface ITaskService
{
    Task<IReadOnlyCollection<TaskResponse>> GetAllAsync(string? status, CancellationToken cancellationToken = default);
    Task<TaskResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskResponse> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
