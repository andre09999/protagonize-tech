using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Common.Exceptions;
using TaskManager.Api.Contracts.Auth;
using TaskManager.Api.Data;
using TaskManager.Api.Entities;
using TaskManager.Api.Infrastructure.Auth;

namespace TaskManager.Api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        IPasswordHasher<AppUser> passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();
        var normalizedUsername = username.ToUpperInvariant();

        var userAlreadyExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(user => user.NormalizedUsername == normalizedUsername, cancellationToken);

        if (userAlreadyExists)
        {
            throw new ConflictException("Ja existe um usuario cadastrado com esse nome.");
        }

        var user = new AppUser
        {
            Username = username,
            NormalizedUsername = normalizedUsername,
            DataCriacao = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Novo usuario cadastrado com sucesso. UserId={UserId} Username={Username}", user.Id, user.Username);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = request.Username.Trim().ToUpperInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(item => item.NormalizedUsername == normalizedUsername, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Usuario ou senha invalidos.");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Usuario ou senha invalidos.");
        }

        _logger.LogInformation("Usuario autenticado com sucesso. UserId={UserId} Username={Username}", user.Id, user.Username);

        return BuildAuthResponse(user);
    }

    private AuthResponse BuildAuthResponse(AppUser user)
    {
        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse(
            token,
            expiresAtUtc,
            new AuthUserResponse(user.Id, user.Username));
    }
}
