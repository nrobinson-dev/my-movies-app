using System.ComponentModel.DataAnnotations;

namespace MyMoviesApp.Api.Models;

public class MovieSearchRequest
{
    [MaxLength(100, ErrorMessage = "Search query cannot exceed 100 characters.")]
    public string Search { get; set; } = string.Empty;
    
    [Range(1, 500, ErrorMessage = "Page number must be between 1 and 500.")]
    public int Page { get; set; } = 1;
}