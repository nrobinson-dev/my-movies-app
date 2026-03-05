using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Common.Dtos;

public class MovieSummaryCollectionDto
{
    public MovieSummaryCollectionDto(IEnumerable<MovieSummaryDto> movieSummaryCollection)
    {
        var summaries = movieSummaryCollection as MovieSummaryDto[] ?? movieSummaryCollection.ToArray();
        Movies = [..summaries];
        TotalCount = summaries.Length;
        TotalDvdCount = summaries.Count(ms => ms.Formats.Contains(Format.Dvd));
        TotalBluRayCount = summaries.Count(ms => ms.Formats.Contains(Format.BluRay));
        TotalBluRay4KCount = summaries.Count(ms => ms.Formats.Contains(Format.BluRay4K));
        TotalDigitalCount = summaries.Count(ms => ms.DigitalRetailers.Any());
    }

    public List<MovieSummaryDto> Movies { get; }
    public int TotalCount { get; }
    public int TotalDvdCount { get; }
    public int TotalBluRayCount { get; }
    public int TotalBluRay4KCount { get; }
    public int TotalDigitalCount { get; }
}

public record MovieSummaryDto(
    int TmdbId,
    string Title,
    DateOnly ReleaseDate,
    string PosterPath,
    List<Format> Formats,
    List<DigitalRetailer> DigitalRetailers)
{
    public static MovieSummaryDto FromDomain(MovieSummary m) =>
        new(m.MovieId, m.Title, m.ReleaseDate, m.PosterPath, m.Formats, m.DigitalRetailers);
}

