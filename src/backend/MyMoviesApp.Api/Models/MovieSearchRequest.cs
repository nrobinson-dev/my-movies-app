namespace MyMoviesApp.Api.Models;

public class MovieSearchRequest
{
    public Guid? UserId { get; set; }
    public string? Search { get; set; } = string.Empty;
    public string? Page { get; set; } = "1";
}