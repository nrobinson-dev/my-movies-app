using System.ComponentModel.DataAnnotations;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Domain.Entities;

/// <summary>
/// Collection of MovieSummary entities, which can represent TMDB search results or movie ownership.
/// </summary>
public class MovieSummaryCollection(List<MovieSummary> movies)
{
    public List<MovieSummary> Movies { get; } = movies;
    public int TotalDvdCount { get; set; } = 0;
    public int TotalBluRayCount { get; set; } = 0;
    public int TotalBluRay4KCount { get; set; } = 0;
    public int TotalDigitalCount { get; set; } = 0;
    public int Page { get; set; } = 0;
    public int TotalPages { get; set; } = 0;
    public int TotalResults { get; set; } = 0;
};

public class MovieSummary
{
    [Range(1, int.MaxValue)]
    public int MovieId { get; set; }
    [RegularExpression(@"^\S.*\S$")]
    public string Title { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    [RegularExpression(@"^\/.*\.jpg$")]
    public string PosterPath { get; set; } = string.Empty;
    public List<UserMovieFormat> Formats { get; set; } = [];
    public List<UserMovieDigitalRetailer> DigitalRetailers { get; set; } = [];
};

public class SaveMovieSummary
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
    public List<UserMovieFormat> Formats { get; set; } = new();
    public List<UserMovieDigitalRetailer> DigitalRetailers { get; set; } = new();
}

public class UserMovieFormat
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UserMovieDigitalRetailer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}