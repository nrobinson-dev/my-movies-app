namespace MyMoviesApp.Presentation.WebAPI.Models;

public class MovieSearchRequest
{
    public string? Search { get; set; } = string.Empty;
    public string? Page { get; set; } = "1";
}