using FluentAssertions;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.Movies.Queries;
using MyMoviesApp.Application.Tests.Common;
using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Application.Tests.Features.Movies.Queries;

public class GetMovieSearchResultsQueryHandlerTests
{
    private readonly Mock<ITmdbService> _tmdbServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly GetMovieSearchResultsQueryHandler _handler;
    
    public GetMovieSearchResultsQueryHandlerTests()
    {
        _handler = new GetMovieSearchResultsQueryHandler(_tmdbServiceMock.Object, _userRepositoryMock.Object);
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
            .ReturnsAsync(new MovieSummaryCollection{Movies = movies});

        var query = new GetMovieSearchResultsQuery("matrix", null, "1");

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
            .ReturnsAsync(new MovieSummaryCollection());

        var query = new GetMovieSearchResultsQuery("unknown movie", null, "1");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Movies.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_ForwardPageParameter_ToTmdbService()
    {
        // Arrange
        _tmdbServiceMock
            .Setup(s => s.SearchMoviesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(new MovieSummaryCollection());

        var query = new GetMovieSearchResultsQuery("action", null, Page: "3");

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
        var userId = Guid.NewGuid();
        var movie = new MovieSummary
        {
            MovieId = 10,
            Title = "Dune",
            ReleaseDate = new DateOnly(2021, 10, 22),
            PosterPath = "/dune.jpg"
        };

        var userMovie = new MovieSummary
        {
            MovieId = 10,
            Title = "Dune",
            ReleaseDate = new DateOnly(2021, 10, 22),
            PosterPath = "/dune.jpg",
            Formats = [TestConstants.Formats.BluRay4K],
            DigitalRetailers = [TestConstants.Retailers.AppleTv]
        };

        _tmdbServiceMock
            .Setup(s => s.SearchMoviesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(new MovieSummaryCollection { Movies = new List<MovieSummary> { movie } });

        _userRepositoryMock
            .Setup(r => r.GetUserMoviesByTmdbIdsAsync(userId, It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MovieSummary> { userMovie });

        var query = new GetMovieSearchResultsQuery("dune", userId, TestConstants.Pagination.DefaultPageNumber.ToString());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultMovie = result.Movies.FirstOrDefault();
        resultMovie.Should().NotBeNull();
        resultMovie!.Formats.Should().Contain(TestConstants.Formats.BluRay4KItem);
        resultMovie.DigitalRetailers.Should().Contain(TestConstants.Retailers.AppleTvItem);
    }
}

