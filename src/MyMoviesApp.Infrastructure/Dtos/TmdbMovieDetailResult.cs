using System.Text.Json.Serialization;

namespace MyMoviesApp.Infrastructure.Dtos;

public class TmdbMovieDetailResult
{
    [JsonPropertyName("adult")]
    public bool Adult { get; set; } = true;

    [JsonPropertyName("backdrop_path")]
    public string BackdropPath { get; set; } = string.Empty;

    [JsonPropertyName("belongs_to_collection")]
    public BelongsToCollection BelongsToCollection { get; set; } = new();

    [JsonPropertyName("budget")]
    public int Budget { get; set; } = 0;

    [JsonPropertyName("genres")]
    public List<TmdbMovieGenre> Genres { get; set; } = new();

    [JsonPropertyName("homepage")]
    public string HomePage { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; } = 0;

    [JsonPropertyName("imdb_id")]
    public string ImdbId { get; set; } = string.Empty;

    [JsonPropertyName("original_language")]
    public string OriginalLanguage { get; set; } = string.Empty;

    [JsonPropertyName("original_title")]
    public string OriginalTitle { get; set; } = string.Empty;

    [JsonPropertyName("overview")]
    public string Overview { get; set; } = string.Empty;

    [JsonPropertyName("popularity")]
    public decimal Popularity { get; set; } = 0;

    [JsonPropertyName("poster_path")]
    public string PosterPath { get; set; } = string.Empty;

    [JsonPropertyName("production_companies")]
    public List<TmdbProductionCompany> ProductionCompanies { get; set; } = new();

    [JsonPropertyName("production_countries")]
    public List<TmdbProductionCountry> ProductionCountries { get; set; } = new();

    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; } = string.Empty;

    [JsonPropertyName("revenue")]
    public int Revenue { get; set; } = 0;

    [JsonPropertyName("runtime")]
    public int Runtime { get; set; } = 0;

    [JsonPropertyName("spoken_languages")]
    public List<TmdbMovieSpokenLanguage> SpokenLanguages { get; set; } = new();

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("tagline")]
    public string Tagline { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("video")]
    public bool Video { get; set; } = false;

    [JsonPropertyName("vote_average")]
    public decimal VoteAverage { get; set; } = 0;

    [JsonPropertyName("vote_count")]
    public int VoteCount { get; set; } = 0;
}

public class BelongsToCollection
{
    [JsonPropertyName("id")]
    public int Id { get; set; } = 0;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("poster_path")]
    public string PosterPath { get; set; } = string.Empty;

    [JsonPropertyName("backdrop_path")]
    public string BackdropPath { get; set; } = string.Empty;
}

public class TmdbMovieGenre{
    [JsonPropertyName("id")]
    public int Id { get; set; } = 0;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TmdbMovieSpokenLanguage
{
    [JsonPropertyName("english_name")]
    public string EnglishName { get; set; } = string.Empty;

    [JsonPropertyName("iso_639_1")]
    public string Iso6391 { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TmdbProductionCompany
{
    [JsonPropertyName("id")]
    public int Id { get; set; } = 0;

    [JsonPropertyName("logo_path")]
    public string LogoPath { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("origin_country")]
    public string OriginCountry { get; set; } = string.Empty;
}

public class TmdbProductionCountry
{
    [JsonPropertyName("iso_3166_1")]
    public string Iso31661 { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}