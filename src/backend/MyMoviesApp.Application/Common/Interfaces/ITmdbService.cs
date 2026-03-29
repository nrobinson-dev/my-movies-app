using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Application.Common.Interfaces;

/// <summary>
/// This interface defines methods for interacting with the TMDb (The Movie Database) API
/// to fetch movie details and search for movies. It abstracts the external API calls, allowing the application to
/// retrieve movie information without being tightly coupled to the TMDb API implementation.
/// </summary>
public interface ITmdbService
{
    /// <summary>
    /// Method to get movie details by its TMDB movie ID. This method will call the TMDb API to fetch detailed
    /// information about a specific movie using its unique identifier from TMDb.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MovieDetail> GetMovieByTmdbMovieIdAsync(int id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Method to search for movies by a search term and optional page number. This method will call the TMDb API to
    /// search for movies that match the provided search term, and it can also handle pagination by accepting a page number.
    /// </summary>
    /// <param name="term"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    Task<MovieSummaryCollection> SearchMoviesAsync(string term, CancellationToken cancellationToken, int page = 1);
}
