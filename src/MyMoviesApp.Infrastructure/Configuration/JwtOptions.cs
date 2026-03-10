using System.ComponentModel.DataAnnotations;

namespace MyMoviesApp.Infrastructure.Configuration;

public class JwtOptions
{
    public const string SectionName = "JwtSettings";
    
    [Required]
    public string Issuer { get; set; } = "MyMoviesApp";
    
    public string Audience { get; set; } = "MyMoviesApp.Client";
    
    [Required][MinLength(1)]
    public string SigningKey { get; set; }
    
    [Required]
    public int AccessTokenExpirationMinutes { get; set; } = 60;
}