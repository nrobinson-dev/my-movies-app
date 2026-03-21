using System.Net.Http.Json;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Infrastructure.Dtos;

namespace MyMoviesApp.Infrastructure.Services;

public class TmdbService(HttpClient httpClient) : ITmdbService
{
    public async Task<MovieDetail> GetMovieByTmdbMovieIdAsync(int id, CancellationToken ct)
    {
        var url = string.Format("movie/{0}", id);
        using var responseMessage = await httpClient.GetAsync(url, ct);
        responseMessage.EnsureSuccessStatusCode();

        var response = await responseMessage.Content.ReadFromJsonAsync<TmdbMovieDetailResult>();

        return response is null ? null! : Map(response);
    }
    
    public async Task<MovieSummaryCollection> SearchMoviesAsync(string term, CancellationToken ct, string page = "1")
    {
        if (string.IsNullOrWhiteSpace(term))
            return new MovieSummaryCollection(new List<MovieSummary>());

        var url = string.Format("search/movie?query={0}&page={1}", Uri.EscapeDataString(term), page);
        using var responseMessage = await httpClient.GetAsync(url, ct);
        responseMessage.EnsureSuccessStatusCode();

        var response = await responseMessage.Content.ReadFromJsonAsync<TmdbSearchMovieResultDto>();

        if (response is null)
            return new MovieSummaryCollection(new List<MovieSummary>());

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
        var result = new MovieSummaryCollection(tmdbSearchMovieResultDto.Results.Select(Map).ToList())
        {
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
