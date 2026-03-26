using Microsoft.EntityFrameworkCore;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Infrastructure.Data.Models;

namespace MyMoviesApp.Infrastructure.Data.Repositories;

public class AuthRepository(MyMoviesAppContext dbcontext) : IAuthRepository
{
    public async Task<int> CreateUserAsync(User user, string passwordHash, CancellationToken cancellationToken)
    {
        var userDb = new UserDb
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
        
        await dbcontext.Users
            .AddAsync(userDb, cancellationToken);
        
        return await dbcontext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetAuthenticatedUserAsync(string userName, string passwordHash, CancellationToken cancellationToken)
    {
        var userDb = await dbcontext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == userName && u.PasswordHash == passwordHash, cancellationToken);
        
        return userDb is null ? null : new User(userDb.Id, userDb.Email);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var userDb = await dbcontext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        
        return userDb is null ? null : new User(userDb.Id, userDb.Email);
    }

    public async Task<(User user, string passwordHash)?> GetUserWithPasswordHashByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var userDb = await dbcontext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        
        if (userDb is null)
            return null;

        var user = new User(userDb.Id, userDb.Email);
        return (user, userDb.PasswordHash);
    }

    public async Task<int> DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbcontext.Users
            .Where(u => u.Id == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}