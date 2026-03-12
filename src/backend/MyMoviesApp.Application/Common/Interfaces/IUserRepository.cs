using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Application.Common.Interfaces;

/// <summary>
/// User repository interface for managing user data and their associated movies. This includes operations for
/// creating users, retrieving users by email, saving user movie summaries, and fetching a user's movie collection.
/// </summary>
public interface IUserRepository
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
    /// Returns the movie formats and digital retailers for a single movie in a user's collection.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="movieId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<UserMovieFormatsAndDigitalRetailers> GetUserMovieFormatsAndDigitalRetailersAsync(Guid userId, int movieId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Saves a movie summary to the user's collection.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="saveMovieSummary"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> SaveUserMovieAsync(Guid userId, SaveMovieSummary saveMovieSummary, CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets the collection of movie summaries associated with the user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MovieSummaryCollection> GetUserMoviesAsync(Guid userId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Deletes a movie from the user's collection based on the TMDB movie ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tmdbId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteUserMovieAsync(Guid userId, int tmdbId, CancellationToken cancellationToken);
}