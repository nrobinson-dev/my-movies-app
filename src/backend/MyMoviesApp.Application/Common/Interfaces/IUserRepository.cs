using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Application.Common.Interfaces;

/// <summary>
/// User repository interface for managing user data and their associated movies. This includes operations for
/// creating users, retrieving users by email, saving user movie summaries, and fetching a user's movie collection.
/// </summary>
public interface IUserRepository
{
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
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>s
    Task<MovieSummaryCollection> GetUserMoviesAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    
    /// <summary>
    /// Deletes a movie from the user's collection based on the TMDB movie ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tmdbId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteUserMovieAsync(Guid userId, int tmdbId, CancellationToken cancellationToken);
}