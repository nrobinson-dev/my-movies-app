using System.ComponentModel.DataAnnotations;

namespace MyMoviesApp.Infrastructure.Configuration;

public class TmdbOptions
{
    public const string SectionName = "TmdbSettings";
    
    [Required]
    public string ApiBaseUrl { get; set; } = "https://api.themoviedb.org/3";
    
    [Required][MinLength(1)]
    public string BearerToken { get; set; }
}
