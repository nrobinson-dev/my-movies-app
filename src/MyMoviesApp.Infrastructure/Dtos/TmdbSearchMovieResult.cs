using System.Text.Json.Serialization;

namespace MyMoviesApp.Infrastructure.Dtos;

public class TmdbSearchMovieResultDto
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("results")]
    public List<TmdbSearchMovieDetailDto> Results { get; set; } = new();
    
    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }
    
    [JsonPropertyName("total_results")]
    public int TotalResults { get; set; }
};

public class TmdbSearchMovieDetailDto
{

    [JsonPropertyName("adult")]
    public bool Adult { get; init; } = true;
    
    [JsonPropertyName("backdrop_path")]
    public string BackdropPath { get; init; } = string.Empty;
    
    [JsonPropertyName("genre_ids")]
    public List<int> GenreIds { get; init; } = new();
    
    [JsonPropertyName("id")]
    public int Id { get; init; } = 0;
    
    [JsonPropertyName("original_language")]
    public string OriginalLanguage { get; init; } = string.Empty;
    
    [JsonPropertyName("original_title")]
    public string OriginalTitle { get; init; } = string.Empty;
    
    [JsonPropertyName("overview")]
    public string Overview { get; init; } = string.Empty;
    
    [JsonPropertyName("popularity")]
    public decimal Popularity { get; init; } = 0;
    
    [JsonPropertyName("poster_path")]
    public string PosterPath { get; init; } = string.Empty;
    
    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; init; } = string.Empty;
    
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;
    
    [JsonPropertyName("video")]
    public bool Video { get; init; } = true;
    
    [JsonPropertyName("vote_average")]
    public decimal VoteAverage { get; init; } = 0;
    
    [JsonPropertyName("vote_count")]
    public int VoteCount { get; init; } = 0;
};