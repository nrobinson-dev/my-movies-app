namespace MyMoviesApp.Infrastructure.Configuration;

public class TmdbOptions
{
    public const string SectionName = "TmdbSettings";
    public string ApiBaseUrl { get; init; } = "https://api.themoviedb.org/3";
    public string BearerToken { get; init; } = string.Empty;
}
