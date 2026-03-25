using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MyMoviesApp.Infrastructure.Services;

namespace MyMoviesApp.Infrastructure.Tests.Services.Integration
{
    [Trait("Category","Integration")]
    public class TmdbServiceIntegrationTests
    {
        private readonly TmdbService _service;
        private readonly TmdbService _unauthorizedService;

        public TmdbServiceIntegrationTests()
        {
            var key = Environment.GetEnvironmentVariable("TMDB_ACCESS_TOKEN");
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("TMDB_ACCESS_TOKEN not set");
            }

            var client = new HttpClient { BaseAddress = new Uri("https://api.themoviedb.org/3/") };
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
            _service = new TmdbService(client, NullLogger<TmdbService>.Instance);

            var unauthorizedClient = new HttpClient { BaseAddress = new Uri("https://api.themoviedb.org/3/") };
            unauthorizedClient.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-token");
            _unauthorizedService = new TmdbService(unauthorizedClient, NullLogger<TmdbService>.Instance);
        }

        // ...existing code...

        [Fact]
        public async Task GetMovieByTmdbMovieIdAsync_WithRealApi_ReturnsExpected()
        {
            // Arrange
            var movieId = 550;

            // Act
            var movie = await _service.GetMovieByTmdbMovieIdAsync(movieId, CancellationToken.None);

            // Assert
            movie.Should().NotBeNull();
            movie.Title.Should().Be("Fight Club");
        }

        [Fact]
        public async Task GetMovieByTmdbMovieIdAsync_ShouldThrow_WhenCancellationRequested()
        {
            // Arrange
            var movieId = 550;
            var cts = new CancellationTokenSource();

            // Act + Assert
            cts.Cancel();
            Func<Task> act = async () => await _service.GetMovieByTmdbMovieIdAsync(movieId, cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task SearchMoviesAsync_WithRealApi_ReturnsSomeResults()
        {
            // Arrange
            var searchTerm = "batman";

            // Act
            var results = await _service.SearchMoviesAsync(searchTerm, CancellationToken.None);

            // Assert
            results.Should().NotBeNull();
            results.Movies.Should().NotBeEmpty();
        }

        [Fact]
        public async Task SearchMoviesAsync_ShouldThrow_WhenCancellationRequested()
        {
            // Arrange
            var searchTerm = "batman";
            var cts = new CancellationTokenSource();

            // Act + Assert
            cts.Cancel();
            Func<Task> act = async () => await _service.SearchMoviesAsync(searchTerm, cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task GetMovieByTmdbMovieIdAsync_ShouldThrow_WhenTokenIsInvalid()
        {
            // Arrange
            var movieId = 550;
            
            // Act
            var act = async () => await _unauthorizedService.GetMovieByTmdbMovieIdAsync(movieId, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .Where(ex => ex.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SearchMoviesAsync_ShouldThrow_WhenTokenIsInvalid()
        {
            // Arrange
            var searchTerm = "batman";
            
            // Act
            var act = async () => await _unauthorizedService.SearchMoviesAsync(searchTerm, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .Where(ex => ex.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        }
    }
}