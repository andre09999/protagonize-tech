using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Entities;

namespace TaskManager.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");

            entity.HasKey(task => task.Id);

            entity.Property(task => task.Titulo)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(task => task.Descricao)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(task => task.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(task => task.DataCriacao)
                .IsRequired();
        });
    }
}
