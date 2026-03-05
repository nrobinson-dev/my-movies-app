using System.ComponentModel.DataAnnotations;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Features.User.Dtos;

public class SaveUserMovieOwnershipDto
{
    [Required][Range(1, int.MaxValue)]
    public int TmdbId { get; init; } = 0;

    [Required][MinLength(1)]
    public string Title { get; init; } = string.Empty;

    [Required][RegularExpression("^\\d{4}-\\d{2}-\\d{2}$")]
    public DateOnly ReleaseDate { get; init; } = DateOnly.MinValue;

    [Required][RegularExpression(@"^\/.*\.jpg$")]
    public string PosterPath { get; init; } = string.Empty;

    [Required]
    public HashSet<Format> Formats { get; init; } = new();

    [Required]
    public HashSet<DigitalRetailer> DigitalRetailers { get; init; } = new();
}