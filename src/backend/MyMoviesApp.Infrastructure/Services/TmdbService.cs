using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Infrastructure.Dtos;

namespace MyMoviesApp.Infrastructure.Services;

public class TmdbService(HttpClient httpClient, ILogger<TmdbService> logger) : ITmdbService
{
    public async Task<MovieDetail> GetMovieByTmdbMovieIdAsync(int id, CancellationToken ct)
    {
        var url = $"movie/{id}";
        logger.LogDebug("Fetching movie details from TMDB. TmdbId: {TmdbId}", id);

        using var responseMessage = await httpClient.GetAsync(url, ct);
        responseMessage.EnsureSuccessStatusCode();

        var response = await responseMessage.Content.ReadFromJsonAsync<TmdbMovieDetailResult>(ct);

        if (response is null)
        {
            logger.LogWarning("TMDB returned a null or undeserializable response for movie detail. TmdbId: {TmdbId}", id);
            return null!;
        }

        return Map(response);
    }
    
    public async Task<MovieSummaryCollection> SearchMoviesAsync(string term, CancellationToken ct, string page = "1")
    {
        if (string.IsNullOrWhiteSpace(term))
            return new MovieSummaryCollection();

        var url = $"search/movie?query={Uri.EscapeDataString(term)}&page={page}";
        logger.LogDebug("Searching TMDB movies. Term: {Term}, Page: {Page}", term, page);

        using var responseMessage = await httpClient.GetAsync(url, ct);
        responseMessage.EnsureSuccessStatusCode();

        var response = await responseMessage.Content.ReadFromJsonAsync<TmdbSearchMovieResultDto>(ct);

        if (response is null)
        {
            logger.LogWarning("TMDB returned a null or undeserializable response for movie search. Term: {Term}", term);
            return new MovieSummaryCollection();
        }

        return Map(response);
    }


    private MovieDetail Map(TmdbMovieDetailResult movie)
    {
        return new MovieDetail(
             MovieId: movie.Id,
             Title: movie.Title,
             ReleaseDate: TryParseReleaseDate(movie.ReleaseDate) ?? default,
             Runtime: movie.Runtime,
             PosterPath: movie.PosterPath,
             BackdropPath: movie.BackdropPath,
             Tagline: movie.Tagline,
             Overview: movie.Overview);
    }

    private MovieSummary Map(TmdbSearchMovieDetailDto tmdbSearchMovieDetailDto)
    {
        return new MovieSummary
        {
            MovieId = tmdbSearchMovieDetailDto.Id,
            Title = tmdbSearchMovieDetailDto.Title,
            ReleaseDate = TryParseReleaseDate(tmdbSearchMovieDetailDto.ReleaseDate) ?? default,
            PosterPath = tmdbSearchMovieDetailDto.PosterPath
        };
    }

    private MovieSummaryCollection Map(TmdbSearchMovieResultDto tmdbSearchMovieResultDto)
    {
        var result = new MovieSummaryCollection
        {
            Movies = tmdbSearchMovieResultDto.Results.Select(Map).ToList(),
            Page = tmdbSearchMovieResultDto.Page,
            TotalPages = tmdbSearchMovieResultDto.TotalPages,
            TotalResults = tmdbSearchMovieResultDto.TotalResults
        };
        
        return result;
    }

    private DateOnly? TryParseReleaseDate(string? releaseDate)
    {
        if (string.IsNullOrWhiteSpace(releaseDate))
        {
            return null;
        }

        if (DateOnly.TryParse(releaseDate, out var parsedDate))
        {
            return parsedDate;
        }

        return null;
    }
}
