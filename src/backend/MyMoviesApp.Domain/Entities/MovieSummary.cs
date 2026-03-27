using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Domain.Entities;

/// <summary>
/// Collection of MovieSummary entities, which can represent TMDB search results or movie ownership.
/// </summary>
public class MovieSummaryCollection()
{
    public List<MovieSummary> Movies { get; set; } = [];
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
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    public string PosterPath { get; set; } = string.Empty;
    public List<Format> Formats { get; set; } = [];
    public List<DigitalRetailer> DigitalRetailers { get; set; } = [];
};
