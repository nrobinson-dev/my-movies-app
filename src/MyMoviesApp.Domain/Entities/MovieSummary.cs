using System.ComponentModel.DataAnnotations;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Domain.Entities;

/// <summary>
/// Collection of MovieSummary entities, which can represent TMDB search results or movie ownership.
/// </summary>
public record MovieSummaryCollection(List<MovieSummary>? Movies)
{
    public List<MovieSummary> Movies { get; } = Movies?.ToList() ?? new List<MovieSummary>();
    public int TotalCount { get; } = Movies?.Count() ?? 0;
    public int TotalDvdCount { get; } = Movies?.Count(ms => ms.Formats.Contains(Format.Dvd)) ?? 0;
    public int TotalBluRayCount { get; } = Movies?.Count(ms => ms.Formats.Contains(Format.BluRay)) ?? 0;
    public int TotalBluRay4KCount { get; } = Movies?.Count(ms => ms.Formats.Contains(Format.BluRay4K)) ?? 0;
    public int TotalDigitalCount { get; } = Movies?.Count(ms => ms.DigitalRetailers.Any()) ?? 0;
};

/// <summary>
/// Formats and DigitalRetailers can be null, otherwise they need to be a hash set.
/// </summary>
public class MovieSummary
{
    [Range(1, int.MaxValue)]
    public int MovieId { get; set; }
    [RegularExpression(@"^\S.*\S$")]
    public string Title { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    [RegularExpression(@"^\/.*\.jpg$")]
    public string PosterPath { get; set; } = string.Empty;
    public List<Format> Formats { get; set; } = [];
    public List<DigitalRetailer> DigitalRetailers { get; set; } = [];
};

/// <summary>
/// Specifically for the GetUserMovieByMovieId endpoint, which will then be added to the MovieDetailDto.
/// </summary>
public class UserMovieFormatsAndDigitalRetailers
{
    public List<Format> Formats { get; set; } = new();
    public List<DigitalRetailer> DigitalRetailers { get; set; } = new();
}