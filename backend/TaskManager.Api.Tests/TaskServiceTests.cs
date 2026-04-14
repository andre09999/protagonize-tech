using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManager.Api.Common.Exceptions;
using TaskManager.Api.Contracts.Tasks;
using TaskManager.Api.Data;
using TaskManager.Api.Entities;
using TaskManager.Api.Services.Tasks;

namespace TaskManager.Api.Tests;

public class TaskServiceTests
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnCachedTasks_WhenCacheHit()
    {
        await using var context = CreateContext();
        var currentUser = new FakeCurrentUserService { UserId = 7, Username = "andre" };
        var cache = new FakeTaskCacheService();
        var publisher = new FakeTaskEventPublisher();
        var expectedTasks = new[]
        {
            new TaskResponse(1, "Do cache", "Tarefa cacheada", "Pendente", DateTime.UtcNow)
        };

        cache.SeedTaskList(currentUser.UserId, null, expectedTasks);

        var service = CreateService(context, currentUser, cache, publisher);

        var response = await service.GetAllAsync(null);

        response.Should().BeEquivalentTo(expectedTasks);
        publisher.PublishedMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistTaskInvalidateCacheAndPublishEvent()
    {
        await using var context = CreateContext();
        var currentUser = await SeedUserAsync(context, 15, "maria");
        var cache = new FakeTaskCacheService();
        var publisher = new FakeTaskEventPublisher();
        var service = CreateService(context, currentUser, cache, publisher);

        var response = await service.CreateAsync(new CreateTaskRequest
        {
            Titulo = "Implementar login",
            Descricao = "Conectar tela de autenticacao na API",
            Status = "Pendente"
        });

        response.Id.Should().BeGreaterThan(0);
        response.Titulo.Should().Be("Implementar login");

        var savedTask = await context.Tasks.SingleAsync();
        savedTask.AppUserId.Should().Be(currentUser.UserId);
        cache.InvalidatedKeys.Should().Contain($"tasks:user:{currentUser.UserId}:list:");
        publisher.PublishedMessages.Should().ContainSingle(message =>
            message.Action == "created" &&
            message.TaskId == response.Id &&
            message.UserId == currentUser.UserId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTaskAndPublishEvent()
    {
        await using var context = CreateContext();
        var currentUser = await SeedUserAsync(context, 9, "joao");
        var cache = new FakeTaskCacheService();
        var publisher = new FakeTaskEventPublisher();

        context.Tasks.Add(new TaskItem
        {
            Id = 42,
            Titulo = "Versao inicial",
            Descricao = "Descricao antiga",
            Status = "Pendente",
            DataCriacao = DateTime.UtcNow.AddHours(-2),
            AppUserId = currentUser.UserId
        });
        await context.SaveChangesAsync();

        var service = CreateService(context, currentUser, cache, publisher);

        var response = await service.UpdateAsync(42, new UpdateTaskRequest
        {
            Titulo = "Versao final",
            Descricao = "Descricao nova",
            Status = "Concluida"
        });

        response.Status.Should().Be("Concluida");

        var savedTask = await context.Tasks.SingleAsync();
        savedTask.Titulo.Should().Be("Versao final");
        savedTask.Descricao.Should().Be("Descricao nova");
        cache.InvalidatedKeys.Should().Contain($"tasks:user:{currentUser.UserId}:item:42");
        publisher.PublishedMessages.Should().ContainSingle(message => message.Action == "updated");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFound_WhenTaskBelongsToAnotherUser()
    {
        await using var context = CreateContext();
        await SeedUserAsync(context, 1, "andre");
        context.Tasks.Add(new TaskItem
        {
            Id = 8,
            Titulo = "Privada",
            Descricao = "Nao pode ser removida por outro usuario",
            Status = "Pendente",
            DataCriacao = DateTime.UtcNow,
            AppUserId = 999
        });
        await context.SaveChangesAsync();

        var service = CreateService(
            context,
            new FakeCurrentUserService { UserId = 1, Username = "andre" },
            new FakeTaskCacheService(),
            new FakeTaskEventPublisher());

        var action = async () => await service.DeleteAsync(8);

        await action.Should().ThrowAsync<NotFoundException>();
    }

    private static TaskService CreateService(
        AppDbContext context,
        FakeCurrentUserService currentUser,
        FakeTaskCacheService cache,
        FakeTaskEventPublisher publisher) =>
        new(
            context,
            currentUser,
            cache,
            publisher,
            NullLogger<TaskService>.Instance);

    private static async Task<FakeCurrentUserService> SeedUserAsync(AppDbContext context, int userId, string username)
    {
        context.Users.Add(new AppUser
        {
            Id = userId,
            Username = username,
            NormalizedUsername = username.ToUpperInvariant(),
            PasswordHash = "hash",
            DataCriacao = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        return new FakeCurrentUserService { UserId = userId, Username = username };
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
