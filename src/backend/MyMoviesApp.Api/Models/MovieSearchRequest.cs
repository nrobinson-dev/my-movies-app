namespace MyMoviesApp.Api.Models;

public class MovieSearchRequest
{
    public string? Search { get; set; } = string.Empty;
    public int? Page { get; set; } = 1;
}