using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Common.Models;
using TaskManager.Api.Data;
using TaskManager.Api.Entities;

namespace TaskManager.Api.Infrastructure.Database;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");
        var dbContext = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher<AppUser>>();

        var demoUsers = GetDemoUsers();
        var createdUsers = 0;
        var createdTasks = 0;

        foreach (var demoUser in demoUsers)
        {
            var normalizedUsername = demoUser.Username.ToUpperInvariant();
            var existingUser = await dbContext.Users
                .Include(user => user.Tasks)
                .FirstOrDefaultAsync(user => user.NormalizedUsername == normalizedUsername, cancellationToken);

            if (existingUser is null)
            {
                existingUser = new AppUser
                {
                    Username = demoUser.Username,
                    NormalizedUsername = normalizedUsername,
                    DataCriacao = demoUser.CreatedAtUtc
                };

                existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, demoUser.Password);
                dbContext.Users.Add(existingUser);
                await dbContext.SaveChangesAsync(cancellationToken);
                createdUsers++;
            }

            if (existingUser.Tasks.Count > 0)
            {
                continue;
            }

            foreach (var task in demoUser.Tasks)
            {
                dbContext.Tasks.Add(new TaskItem
                {
                    Titulo = task.Titulo,
                    Descricao = task.Descricao,
                    Status = task.Status,
                    DataCriacao = task.DataCriacaoUtc,
                    AppUserId = existingUser.Id
                });

                createdTasks++;
            }
        }

        if (createdTasks > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (createdUsers == 0 && createdTasks == 0)
        {
            logger.LogInformation("Povoamento do banco ignorado porque os dados de demonstracao ja existem.");
            return;
        }

        logger.LogInformation(
            "Povoamento do banco concluido com sucesso. UsuariosCriados={CreatedUsers} TarefasCriadas={CreatedTasks}",
            createdUsers,
            createdTasks);
    }

    private static IReadOnlyCollection<DemoUserSeed> GetDemoUsers()
    {
        var utcNow = DateTime.UtcNow;

        return
        [
            new DemoUserSeed(
                "admin.demo",
                "Admin@123",
                utcNow.AddDays(-15),
                [
                    new DemoTaskSeed("Planejar entrega inicial", "Mapear backlog tecnico e riscos do projeto.", TaskStatuses.Concluida, utcNow.AddDays(-14)),
                    new DemoTaskSeed("Configurar observabilidade", "Validar logs estruturados, health check e rastreabilidade de erros.", TaskStatuses.Pendente, utcNow.AddDays(-6)),
                    new DemoTaskSeed("Revisar arquitetura Docker", "Garantir compose com API, frontend, SQL Server, Redis e RabbitMQ.", TaskStatuses.Pendente, utcNow.AddDays(-2))
                ]),
            new DemoUserSeed(
                "analista.demo",
                "Analista@123",
                utcNow.AddDays(-10),
                [
                    new DemoTaskSeed("Cadastrar cenarios de teste", "Cobrir fluxo de autenticacao, validacao e CRUD de tarefas.", TaskStatuses.Concluida, utcNow.AddDays(-9)),
                    new DemoTaskSeed("Avaliar UX do dashboard", "Revisar alternancia entre modo claro e modo escuro.", TaskStatuses.Pendente, utcNow.AddDays(-4)),
                    new DemoTaskSeed("Publicar evento de auditoria", "Checar mensagens de criacao e atualizacao na fila RabbitMQ.", TaskStatuses.Concluida, utcNow.AddDays(-1))
                ])
        ];
    }

    private sealed record DemoUserSeed(
        string Username,
        string Password,
        DateTime CreatedAtUtc,
        IReadOnlyCollection<DemoTaskSeed> Tasks);

    private sealed record DemoTaskSeed(
        string Titulo,
        string Descricao,
        string Status,
        DateTime DataCriacaoUtc);
}
