using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Application.Common.Interfaces;

public interface IAuthRepository
{
    /// <summary>
    /// Creates a new user with the provided user information and password hash.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="passwordHash"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> CreateUserAsync(User user, string passwordHash, CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets the user by their email address.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets a tuple containing the user and their password hash by email address.
    /// This is used for verifying credentials during login.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Tuple of (User, PasswordHash) or null if user not found</returns>
    Task<(User user, string passwordHash)?> GetUserWithPasswordHashByEmailAsync(string email, CancellationToken cancellationToken);
    
    /// <summary>
    /// Deletes the user with the specified ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The number of rows affected.</returns>
    Task<int> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
}