using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManager.Api.Common.Exceptions;
using TaskManager.Api.Contracts.Auth;
using TaskManager.Api.Data;
using TaskManager.Api.Entities;
using TaskManager.Api.Services.Auth;

namespace TaskManager.Api.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndReturnToken()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var response = await service.RegisterAsync(new RegisterRequest
        {
            Username = "andre",
            Password = "senha-super-forte"
        });

        response.AccessToken.Should().Be("fake-jwt-token");
        response.User.Username.Should().Be("andre");

        var savedUser = await context.Users.SingleAsync();
        savedUser.NormalizedUsername.Should().Be("ANDRE");
        savedUser.PasswordHash.Should().NotBe("senha-super-forte");
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowConflict_WhenUsernameAlreadyExists()
    {
        await using var context = CreateContext();
        context.Users.Add(new AppUser
        {
            Username = "Andre",
            NormalizedUsername = "ANDRE",
            PasswordHash = "hash",
            DataCriacao = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var action = async () => await service.RegisterAsync(new RegisterRequest
        {
            Username = "andre",
            Password = "senha-super-forte"
        });

        await action.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenPasswordIsInvalid()
    {
        await using var context = CreateContext();
        var passwordHasher = new PasswordHasher<AppUser>();
        var user = new AppUser
        {
            Username = "andre",
            NormalizedUsername = "ANDRE",
            DataCriacao = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "senha-correta");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(
            context,
            passwordHasher,
            new StubJwtTokenGenerator(),
            NullLogger<AuthService>.Instance);

        var action = async () => await service.LoginAsync(new LoginRequest
        {
            Username = "andre",
            Password = "senha-incorreta"
        });

        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static AuthService CreateService(AppDbContext context) =>
        new(
            context,
            new PasswordHasher<AppUser>(),
            new StubJwtTokenGenerator(),
            NullLogger<AuthService>.Instance);

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
