using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Application.Common.Interfaces;

/// <summary>
/// Handles authentication operations: registering users, logging in, and verifying credentials.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new user with email and password.
    /// </summary>
    Task<(User user, string token)> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user and returns a JWT token if credentials are valid.
    /// </summary>
    Task<(User user, string token)?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the user with the specified ID.
    /// </summary>
    Task<int> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}