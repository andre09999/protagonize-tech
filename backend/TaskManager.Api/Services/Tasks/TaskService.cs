using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Common.Exceptions;
using TaskManager.Api.Common.Models;
using TaskManager.Api.Contracts.Tasks;
using TaskManager.Api.Data;
using TaskManager.Api.Entities;
using TaskManager.Api.Infrastructure.Caching;
using TaskManager.Api.Infrastructure.CurrentUser;
using TaskManager.Api.Infrastructure.Messaging;

namespace TaskManager.Api.Services.Tasks;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITaskCacheService _taskCacheService;
    private readonly ITaskEventPublisher _taskEventPublisher;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        AppDbContext context,
        ICurrentUserService currentUserService,
        ITaskCacheService taskCacheService,
        ITaskEventPublisher taskEventPublisher,
        ILogger<TaskService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _taskCacheService = taskCacheService;
        _taskEventPublisher = taskEventPublisher;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<TaskResponse>> GetAllAsync(string? status, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? null
            : NormalizeStatus(status);

        var cachedTasks = await _taskCacheService.GetTasksAsync(userId, normalizedStatus, cancellationToken);

        if (cachedTasks is not null)
        {
            _logger.LogInformation(
                "Lista de tarefas retornada do cache. UserId={UserId} Status={Status}",
                userId,
                normalizedStatus ?? "Todos");

            return cachedTasks;
        }

        var query = _context.Tasks
            .AsNoTracking()
            .Where(task => task.AppUserId == userId);

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            query = query.Where(task => task.Status == normalizedStatus);
        }

        var tasks = await query
            .OrderByDescending(task => task.DataCriacao)
            .Select(task => ToResponse(task))
            .ToListAsync(cancellationToken);

        await _taskCacheService.SetTasksAsync(userId, normalizedStatus, tasks, cancellationToken);

        return tasks;
    }

    public async Task<TaskResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var cachedTask = await _taskCacheService.GetTaskAsync(userId, id, cancellationToken);

        if (cachedTask is not null)
        {
            _logger.LogInformation("Tarefa {TaskId} retornada do cache para o usuario {UserId}", id, userId);
            return cachedTask;
        }

        var task = await FindTaskAsync(id, userId, cancellationToken);
        var response = ToResponse(task);

        await _taskCacheService.SetTaskAsync(userId, response, cancellationToken);

        return response;
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var username = _currentUserService.Username;

        var task = new TaskItem
        {
            Titulo = request.Titulo.Trim(),
            Descricao = request.Descricao.Trim(),
            Status = NormalizeStatus(request.Status),
            DataCriacao = DateTime.UtcNow,
            AppUserId = userId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        var response = ToResponse(task);

        await _taskCacheService.InvalidateTaskListsAsync(userId, cancellationToken);
        await _taskCacheService.SetTaskAsync(userId, response, cancellationToken);

        await PublishEventAsync("created", response, userId, cancellationToken);

        _logger.LogInformation(
            "Tarefa criada com sucesso. TaskId={TaskId} UserId={UserId} Username={Username}",
            response.Id,
            userId,
            username);

        return response;
    }

    public async Task<TaskResponse> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var task = await FindTaskAsync(id, userId, cancellationToken);

        task.Titulo = request.Titulo.Trim();
        task.Descricao = request.Descricao.Trim();
        task.Status = NormalizeStatus(request.Status);

        await _context.SaveChangesAsync(cancellationToken);

        var response = ToResponse(task);

        await _taskCacheService.InvalidateTaskAsync(userId, id, cancellationToken);
        await _taskCacheService.InvalidateTaskListsAsync(userId, cancellationToken);
        await _taskCacheService.SetTaskAsync(userId, response, cancellationToken);

        await PublishEventAsync("updated", response, userId, cancellationToken);

        _logger.LogInformation("Tarefa atualizada com sucesso. TaskId={TaskId} UserId={UserId}", response.Id, userId);

        return response;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var task = await FindTaskAsync(id, userId, cancellationToken);

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);

        await _taskCacheService.InvalidateTaskAsync(userId, id, cancellationToken);
        await _taskCacheService.InvalidateTaskListsAsync(userId, cancellationToken);

        await _taskEventPublisher.PublishAsync(
            new TaskEventMessage("deleted", task.Id, userId, task.Titulo, task.Status, DateTime.UtcNow),
            cancellationToken);

        _logger.LogInformation("Tarefa removida com sucesso. TaskId={TaskId} UserId={UserId}", task.Id, userId);
    }

    private async Task<TaskItem> FindTaskAsync(int id, int userId, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(item => item.Id == id && item.AppUserId == userId, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException("Tarefa nao encontrada.");
        }

        return task;
    }

    private async Task PublishEventAsync(string action, TaskResponse task, int userId, CancellationToken cancellationToken)
    {
        await _taskEventPublisher.PublishAsync(
            new TaskEventMessage(action, task.Id, userId, task.Titulo, task.Status, DateTime.UtcNow),
            cancellationToken);
    }

    private static TaskResponse ToResponse(TaskItem task) =>
        new(task.Id, task.Titulo, task.Descricao, task.Status, task.DataCriacao);

    private static string NormalizeStatus(string? status) => TaskStatuses.Normalize(status ?? TaskStatuses.Pendente);
}
