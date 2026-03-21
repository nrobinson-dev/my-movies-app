using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Common.Dtos;

public class TmdbMovieSummaryCollectionDto(IEnumerable<MovieSummaryDto> movies)
{
    public IEnumerable<MovieSummaryDto> Movies { get; } = movies;
    public int Page { get; set; } = 0;
    public int TotalPages { get; set; } = 0;
    public int TotalResults { get; set; } = 0;
}

public class MovieSummaryCollectionDto(IEnumerable<MovieSummaryDto> movies)
{
    public IEnumerable<MovieSummaryDto> Movies { get; } = movies;
    public int TotalOwnedCount { get; set; } = 0;
    public int TotalDvdCount { get; set; } = 0;
    public int TotalBluRayCount { get; set; } = 0;
    public int TotalBluRay4KCount { get; set; } = 0;
    public int TotalDigitalCount { get; set; } = 0;
    public int Page { get; set; } = 0;
    public int TotalPages { get; set; } = 0;
    public int TotalResults { get; set; } = 0;
}

public class MovieSummaryDto
{
    public int TmdbId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    public string PosterPath { get; set; } = string.Empty;
    public List<UserMovieFormat> Formats { get; set; } = new();
    public List<UserMovieDigitalRetailer> DigitalRetailers { get; set; } = new();
    
    public static MovieSummaryDto FromDomain(MovieSummary m) =>
        new MovieSummaryDto
        {
            TmdbId = m.MovieId,
            Title = m.Title,
            ReleaseDate = m.ReleaseDate,
            PosterPath = m.PosterPath,
            Formats = m.Formats,
            DigitalRetailers = m.DigitalRetailers
        };
}

