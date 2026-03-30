using System.ComponentModel.DataAnnotations;
using MyMoviesApp.Application.Common.Validation;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Features.User.Dtos;

public class SaveUserMovieOwnershipDto
{
    [Required][Range(1, int.MaxValue)]
    public int TmdbId { get; init; } = 0;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public DateOnly ReleaseDate { get; init; } = DateOnly.MinValue;

    [RegularExpression(@"^\/.*\.jpg$")]
    public string PosterPath { get; init; } = string.Empty;

    [Required]
    [ValidEnumValues]
    public HashSet<Format> Formats { get; init; } = new();

    [Required]
    [ValidEnumValues]
    public HashSet<DigitalRetailer> DigitalRetailers { get; init; } = new();
}