namespace MyMoviesApp.Api.Models;

public class MovieSearchRequest
{
    public string? Search { get; set; } = string.Empty;
    public string? Page { get; set; } = "1";
}