using MyMoviesApp.Application.Common.Models;
using MyMoviesApp.Application.Common.Services;
using MyMoviesApp.Domain.Entities;

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
    public List<UserMovieFormatItem> Formats { get; set; } = new();
    public List<UserMovieDigitalRetailerItem> DigitalRetailers { get; set; } = new();

    public static MovieSummaryDto FromDomain(MovieSummary m) =>
        new MovieSummaryDto
        {
            TmdbId = m.MovieId,
            Title = m.Title,
            ReleaseDate = m.ReleaseDate,
            PosterPath = m.PosterPath,
            Formats = m.Formats
                .Select(f => new UserMovieFormatItem { Id = (int)f, Name = f.ToString() })
                .ToList(),
            DigitalRetailers = m.DigitalRetailers
                .Select(r => new UserMovieDigitalRetailerItem { Id = (int)r, Name = r.ToString() })
                .ToList()
        };
    
    public static MovieSummaryDto FromDomain(MovieSummary m, ITitleFormattingService titleFormattingService) =>
        new MovieSummaryDto
        {
            TmdbId = m.MovieId,
            Title = titleFormattingService.FormatForDisplay(m.Title),
            ReleaseDate = m.ReleaseDate,
            PosterPath = m.PosterPath,
            Formats = m.Formats
                .Select(f => new UserMovieFormatItem { Id = (int)f, Name = f.ToString() })
                .ToList(),
            DigitalRetailers = m.DigitalRetailers
                .Select(r => new UserMovieDigitalRetailerItem { Id = (int)r, Name = r.ToString() })
                .ToList()
        };
}

