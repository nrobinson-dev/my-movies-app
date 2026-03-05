namespace MyMoviesApp.Infrastructure.Configuration;

public class JwtOptions
{
    public const string SectionName = "JwtSettings";
    public string Issuer { get; set; } = "MyMoviesApp";
    public string Audience { get; set; } = "MyMoviesApp.Client";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 60;
}

