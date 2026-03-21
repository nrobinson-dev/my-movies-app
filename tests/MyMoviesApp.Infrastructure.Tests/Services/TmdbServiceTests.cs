using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using Moq.Protected;
using MyMoviesApp.Infrastructure.Dtos;
using MyMoviesApp.Infrastructure.Services;
using System.Text.Json;
using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Infrastructure.Tests.Services;

[Trait("Category", "Mocked HttpClient")]
public class TmdbServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public TmdbServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
    }

    private HttpClient CreateHttpClientWithResponse<T>(T? responseObj, Action<HttpRequestMessage>? onSend = null)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                onSend?.Invoke(req);
                var json = JsonSerializer.Serialize(responseObj);
                var msg = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                return msg;
            });

        var client = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/")
        };
        return client;
    }

    [Fact]
    public async Task GetMovieByTmdbMovieIdAsync_Should_ReturnMappedMovie_WhenResponseIsValid()
    {
        // Arrange
        var releaseDate = new DateOnly(2025, 1, 1);

        var tmdb = new TmdbMovieDetailResult
        {
            Id = 123,
            Title = "Test title",
            ReleaseDate = "2025-01-01",
            Runtime = 100,
            PosterPath = "/poster.jpg",
            BackdropPath = "/backdrop.jpg",
            Tagline = "tag",
            Overview = "overview"
        };

        HttpRequestMessage? request = null;
        var client = CreateHttpClientWithResponse(tmdb, req => request = req);
        var service = new TmdbService(client);

        // Act
        var result = await service.GetMovieByTmdbMovieIdAsync(123, new CancellationToken());

        // Assert
        request.Should().NotBeNull();
        request?.RequestUri?.ToString().Should().EndWith("movie/123");
        result.Should().NotBeNull();
        result.MovieId.Should().Be(123);
        result.Title.Should().Be("Test title");
        result.ReleaseDate.Should().Be(releaseDate);
        result.Runtime.Should().Be(100);
        result.PosterPath.Should().Be("/poster.jpg");
        result.BackdropPath.Should().Be("/backdrop.jpg");
        result.Tagline.Should().Be("tag");
        result.Overview.Should().Be("overview");
    }

    [Fact]
    public async Task GetMovieByTmdbMovieIdAsync_Should_ReturnNull_WhenResponseIsNull()
    {
        // Arrange
        HttpRequestMessage? request = null;
        var client = CreateHttpClientWithResponse<object?>(null, req => request = req);
        var service = new TmdbService(client);

        // Act
        var result = await service.GetMovieByTmdbMovieIdAsync(999, new CancellationToken());

        // Assert
        request.Should().NotBeNull();
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchMoviesAsync_Should_Map_Search_Results_Correctly()
    {
        // Arrange
        var searchResult = new TmdbSearchMovieResultDto
        {
            Results = new List<TmdbSearchMovieDetailDto>
            {
                new()
                {
                    Id = 1,
                    Title = "The Matrix",
                    ReleaseDate = "1999-03-31",
                    PosterPath = "/matrix.jpg"
                }
            }
        };

        HttpRequestMessage? request = null;
        var httpClient = CreateHttpClientWithResponse(searchResult, req => request = req);
        var service = new TmdbService(httpClient);

        // Act
        var result = await service.SearchMoviesAsync("Matrix", new CancellationToken());

        // Assert
        request.Should().NotBeNull();
        request?.RequestUri?.ToString().Should().Contain("search/movie");
        result.Should().NotBeNull();
        result.Movies.Should().HaveCount(1);
        result.Movies.First().Title.Should().Be("The Matrix");
    }

    [Fact]
    public async Task SearchMoviesAsync_UsesPageOneByDefault()
    {
        // Arrange
        var tmdbResult = new TmdbSearchMovieResultDto { Results = new List<TmdbSearchMovieDetailDto>() };

        HttpRequestMessage? captured = null;
        var client = CreateHttpClientWithResponse(tmdbResult, req => captured = req);
        var service = new TmdbService(client);

        // Act
        await service.SearchMoviesAsync("term", new CancellationToken());

        // Assert
        captured.Should().NotBeNull();
        captured?.RequestUri?.ToString().Should().Contain("page=1");
    }

    [Fact]
    public async Task SearchMoviesAsync_ShouldReturnEmptyCollection_WhenResponseIsNull()
    {
        // Arrange
        HttpRequestMessage? captured = null;
        var client = CreateHttpClientWithResponse<object?>(null, req => captured = req);
        var service = new TmdbService(client);

        // Act
        var result = await service.SearchMoviesAsync("anything", new CancellationToken());

        // Assert
        captured.Should().NotBeNull();
        result.Should().BeEquivalentTo(new MovieSummaryCollection());
    }


    [Theory]
    [InlineData("foo bar", "1")]
    [InlineData("other", "2")]
    public async Task SearchMoviesAsync_CallsExpectedEndpointAndReturnsMapped(string term, string page)
    {
        // Arrange
        term = Uri.EscapeDataString(term);

        var tmdbResult = new TmdbSearchMovieResultDto
        {
            Results = new List<TmdbSearchMovieDetailDto>
            {
                new TmdbSearchMovieDetailDto
                {
                    Id = 1,
                    Title = "A",
                    ReleaseDate = "2020-01-01",
                    PosterPath = "/image.jpg"
                }
            }
        };

        var expected = new MovieSummaryCollection
        {
            Movies = new List<MovieSummary>
            {
                new MovieSummary
                {
                    MovieId = 1,
                    Title = "A",
                    ReleaseDate = new DateOnly(2020, 1, 1),
                    PosterPath = "/image.jpg"
                }
            }
        };

        HttpRequestMessage? request = null;
        var client = CreateHttpClientWithResponse(tmdbResult, req => request = req);
        var service = new TmdbService(client);

        // Act
        var result = await service.SearchMoviesAsync(term, new CancellationToken(), page);

        request.Should().NotBeNull();
        request?.RequestUri?.ToString()
            .Should().Contain($"search/movie?query={Uri.EscapeDataString(term)}&page={page}");

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}