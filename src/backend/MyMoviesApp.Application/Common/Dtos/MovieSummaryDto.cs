using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Common.Dtos;

public class MovieSummaryCollectionDto
{
    public MovieSummaryCollectionDto(IEnumerable<MovieSummaryDto> movies)
    {
        Movies = movies;
        TotalOwnedCount = Movies.Count();
        TotalDvdCount = Movies.Count(m => m.Formats.Any(f => f.Id == (int)Format.Dvd));
        TotalBluRayCount = Movies.Count(m => m.Formats.Any(f => f.Id == (int)Format.BluRay));
        TotalBluRay4KCount = Movies.Count(m => m.Formats.Any(f => f.Id == (int)Format.BluRay4K));
        TotalDigitalCount = Movies.Count(m => m.DigitalRetailers.Count > 0);
    }

    public IEnumerable<MovieSummaryDto> Movies { get; }
    public int TotalOwnedCount { get; }
    public int TotalDvdCount { get; }
    public int TotalBluRayCount { get; }
    public int TotalBluRay4KCount { get; }
    public int TotalDigitalCount { get; }
    public int Page { get; set; } = 0;
    public int TotalPages { get; set; } = 0;
    public int TotalResults { get; set; } = 0;
}

public record MovieSummaryDto(
    int TmdbId,
    string Title,
    DateOnly ReleaseDate,
    string PosterPath,
    List<UserMovieFormat> Formats,
    List<UserMovieDigitalRetailer> DigitalRetailers)
{
    public static MovieSummaryDto FromDomain(MovieSummary m) =>
        new(m.MovieId, m.Title, m.ReleaseDate, m.PosterPath, m.Formats, m.DigitalRetailers);
}

