using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Common.Models;

/// <summary>
/// Represents the data needed to save a movie to a user's collection.
/// </summary>
public class SaveMovieSummary
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    public string PosterPath { get; set; } = string.Empty;
    public List<Format> Formats { get; set; } = [];
    public List<DigitalRetailer> DigitalRetailers { get; set; } = [];
}

/// <summary>
/// Represents the formats and digital retailers for a single movie in a user's collection.
/// </summary>
public class UserMovieFormatsAndDigitalRetailers
{
    public List<UserMovieFormatItem> Formats { get; set; } = new();
    public List<UserMovieDigitalRetailerItem> DigitalRetailers { get; set; } = new();
}

public record UserMovieFormatItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public record UserMovieDigitalRetailerItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

