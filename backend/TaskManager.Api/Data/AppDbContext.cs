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
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(user => user.Id);

            entity.Property(user => user.Username)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(user => user.NormalizedUsername)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(user => user.DataCriacao)
                .IsRequired();

            entity.HasIndex(user => user.NormalizedUsername)
                .IsUnique();
        });

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

            entity.HasOne(task => task.AppUser)
                .WithMany(user => user.Tasks)
                .HasForeignKey(task => task.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
