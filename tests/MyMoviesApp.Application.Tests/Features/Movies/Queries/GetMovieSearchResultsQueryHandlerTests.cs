using FluentAssertions;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.Movies.Queries;
using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Application.Tests.Features.Movies.Queries;

public class GetMovieSearchResultsQueryHandlerTests
{
    private readonly Mock<ITmdbService> _tmdbServiceMock = new();
    private readonly GetMovieSearchResultsQueryHandler _handler;
    private readonly UserMovieFormat _dvdFormat = new() { Id = (int)Domain.Enums.Format.Dvd, Name = "DVD" };
    private readonly UserMovieFormat _bluRayFormat = new() { Id = (int)Domain.Enums.Format.BluRay, Name = "Blu-ray" };
    private readonly UserMovieFormat _bluRay4KFormat = new() { Id = (int)Domain.Enums.Format.BluRay4K, Name = "Blu-ray 4K" };
    private readonly UserMovieDigitalRetailer _appleTvRetailer = new() { Id = (int)Domain.Enums.DigitalRetailer.AppleTv, Name = "Apple TV" };
    private readonly UserMovieDigitalRetailer _moviesAnywhereRetailer = new() { Id = (int)Domain.Enums.DigitalRetailer.MoviesAnywhere, Name = "Movies Anywhere" };
    private readonly UserMovieDigitalRetailer _youTubeRetailer = new() { Id = (int)Domain.Enums.DigitalRetailer.YouTube, Name = "YouTube" };
    
    public GetMovieSearchResultsQueryHandlerTests()
    {
        _handler = new GetMovieSearchResultsQueryHandler(_tmdbServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnMappedCollection_WhenResultsExist()
    {
        // Arrange
        var movies = new List<MovieSummary>
        {
            new() { MovieId = 1, Title = "The Matrix", ReleaseDate = new DateOnly(1999, 3, 31), PosterPath = "/matrix.jpg" },
            new() { MovieId = 2, Title = "Inception",  ReleaseDate = new DateOnly(2010, 7, 16), PosterPath = "/inception.jpg" }
        };

        _tmdbServiceMock
            .Setup(s => s.SearchMoviesAsync("matrix", It.IsAny<CancellationToken>(), "1"))
            .ReturnsAsync(new MovieSummaryCollection(movies));

        var query = new GetMovieSearchResultsQuery("matrix");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var moviesResult = result.Movies.ToList();
        
        // Assert
        result.Should().NotBeNull();
        result.Movies.Should().HaveCount(2);
        moviesResult[0].TmdbId.Should().Be(1);
        moviesResult[0].Title.Should().Be("The Matrix");
        moviesResult[1].TmdbId.Should().Be(2);
        moviesResult[1].Title.Should().Be("Inception");
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyCollection_WhenNoResultsFound()
    {
        // Arrange
        _tmdbServiceMock
            .Setup(s => s.SearchMoviesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(new MovieSummaryCollection(new List<MovieSummary>()));

        var query = new GetMovieSearchResultsQuery("unknown movie");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Movies.Should().BeEmpty();
        result.TotalOwnedCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_ForwardPageParameter_ToTmdbService()
    {
        // Arrange
        _tmdbServiceMock
            .Setup(s => s.SearchMoviesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(new MovieSummaryCollection(new List<MovieSummary>()));

        var query = new GetMovieSearchResultsQuery("action", Page: "3");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _tmdbServiceMock.Verify(
            s => s.SearchMoviesAsync("action", It.IsAny<CancellationToken>(), "3"),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_MapFormatsAndDigitalRetailers()
    {
        // Arrange
        var movie = new MovieSummary
        {
            MovieId = 10,
            Title = "Dune",
            ReleaseDate = new DateOnly(2021, 10, 22),
            PosterPath = "/dune.jpg",
            Formats = [_bluRay4KFormat],
            DigitalRetailers = [_appleTvRetailer]
        };

        _tmdbServiceMock
            .Setup(s => s.SearchMoviesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(new MovieSummaryCollection(new List<MovieSummary> { movie }));

        // Act
        var result = await _handler.Handle(new GetMovieSearchResultsQuery("dune"), CancellationToken.None);
        
        // Assert
        result.Movies.FirstOrDefault()?.Formats.Should().Contain(_bluRay4KFormat);
        result.Movies.FirstOrDefault()?.DigitalRetailers.Should().Contain(_appleTvRetailer);
    }
}

