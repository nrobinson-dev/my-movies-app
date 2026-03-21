using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Exceptions;

namespace MyMoviesApp.Infrastructure.Services;

public class AuthenticationService(
    IAuthRepository authRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IAuthenticationService
{
    public async Task<(User user, string token)> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var existingUser = await authRepository.GetUserByEmailAsync(email, cancellationToken);
        if (existingUser is not null)
        {
            throw new DuplicateEmailException(email);
        }

        var passwordHash = passwordHasher.HashPassword(password);

        var user = new User(Guid.NewGuid(), email);
        await authRepository.CreateUserAsync(user, passwordHash, cancellationToken);

        var token = jwtTokenGenerator.GenerateJwtToken(user);

        return (user, token);
    }
    
    public async Task<(User user, string token)?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var userWithHash = await authRepository.GetUserWithPasswordHashByEmailAsync(email, cancellationToken);
        if (userWithHash is null)
            return null;

        var (user, storedPasswordHash) = userWithHash.Value;
        
        if (!passwordHasher.VerifyPassword(password, storedPasswordHash))
            return null;
        
        var jwtToken = jwtTokenGenerator.GenerateJwtToken(user);

        return (user, jwtToken);
    }

    public async Task<int> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await authRepository.DeleteUserAsync(userId, cancellationToken);
    }
}

