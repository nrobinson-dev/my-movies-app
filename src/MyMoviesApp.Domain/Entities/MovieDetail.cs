using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Domain.Entities;

/// <summary>
/// Entity. Formats and DigitalRetailers can be null, otherwise they need to be a hash set.
/// </summary>
public record MovieDetail(
    int MovieId,
    string Title,
    DateOnly ReleaseDate,
    int Runtime,
    string PosterPath,
    string BackdropPath,
    string Tagline,
    string Overview
);