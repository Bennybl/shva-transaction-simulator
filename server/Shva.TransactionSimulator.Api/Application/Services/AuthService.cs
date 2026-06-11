using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shva.TransactionSimulator.Api.Application.Interfaces;
using Shva.TransactionSimulator.Api.Contracts.Requests;
using Shva.TransactionSimulator.Api.Contracts.Responses;
using Shva.TransactionSimulator.Api.Domain.Entities;
using Shva.TransactionSimulator.Api.Infrastructure.Persistence;

namespace Shva.TransactionSimulator.Api.Application.Services;

public sealed class AuthService(
    AppDbContext dbContext,
    IConfiguration configuration,
    IClock clock) : IAuthService
{
    private static readonly PasswordHasher<User> PasswordHasher = new();

    public async Task<AuthResponse?> SignupAsync(SignupRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();

        var exists = await dbContext.Users.AnyAsync(u => u.Username == username, cancellationToken);
        if (exists)
        {
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            CreatedAtUtc = clock.UtcNow.UtcDateTime
        };
        user.PasswordHash = PasswordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(CreateToken(user), user.Username);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();

        var user = await dbContext.Users
            .SingleOrDefaultAsync(u => u.Username == username, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var result = PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return new AuthResponse(CreateToken(user), user.Username);
    }

    private string CreateToken(User user)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            ],
            expires: clock.UtcNow.UtcDateTime.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
