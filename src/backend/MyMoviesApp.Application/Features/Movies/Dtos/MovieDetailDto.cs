using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Features.Movies.Dtos;

public class MovieDetailDto(MovieDetail movieDetail)
{
    public int TmdbId { get; init; } = movieDetail.MovieId;
    public string Title { get; init; } = movieDetail.Title;
    public DateOnly ReleaseDate { get; init; } = movieDetail.ReleaseDate;
    public int Runtime { get; init; } = movieDetail.Runtime;
    public string PosterPath { get; init; } = movieDetail.PosterPath;
    public string BackdropPath { get; init; } = movieDetail.BackdropPath;
    public string Tagline { get; init; } = movieDetail.Tagline;
    public string Overview { get; init; } = movieDetail.Overview;
    public List<UserMovieFormat> Formats { get; set; } = new();
    public List<UserMovieDigitalRetailer> DigitalRetailers { get; set; } = new();
}