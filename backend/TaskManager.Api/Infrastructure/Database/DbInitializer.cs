using TaskManager.Api.Data;

namespace TaskManager.Api.Infrastructure.Database;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
                logger.LogInformation("Banco de dados inicializado com sucesso na tentativa {Attempt}", attempt);
                await DatabaseSeeder.SeedAsync(scope.ServiceProvider, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < 10)
            {
                logger.LogWarning(ex, "Falha ao inicializar o banco na tentativa {Attempt}. Nova tentativa em 5 segundos.", attempt);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await DatabaseSeeder.SeedAsync(scope.ServiceProvider, cancellationToken);
    }
}
