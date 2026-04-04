namespace MyMoviesApp.Infrastructure.Data.Models;

public class UserMovieDb
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public int TmdbId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    public string PosterPath { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    
    // Navigation properties for EF Core
    public ICollection<UserMovieFormat> UserMovieFormats { get; set; } = [];
    public ICollection<UserMovieDigitalRetailer> UserMovieDigitalRetailers { get; set; } = [];
}
