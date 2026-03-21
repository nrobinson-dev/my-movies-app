using FluentAssertions;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.User.Queries;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Tests.Features.User.Queries;

public class GetMovieOwnershipQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly GetMovieOwnershipQueryHandler _handler;
    private readonly UserMovieFormat _dvdFormat = new() { Id = (int)Format.Dvd, Name = "DVD" };
    private readonly UserMovieFormat _bluRayFormat = new() { Id = (int)Format.BluRay, Name = "Blu-ray" };
    private readonly UserMovieFormat _bluRay4KFormat = new() { Id = (int)Format.BluRay4K, Name = "Blu-ray 4K" };
    private readonly UserMovieDigitalRetailer _appleTvRetailer = new() { Id = (int)DigitalRetailer.AppleTv, Name = "Apple TV" };
    private readonly UserMovieDigitalRetailer _moviesAnywhereRetailer = new() { Id = (int)DigitalRetailer.MoviesAnywhere, Name = "Movies Anywhere" };
    private readonly UserMovieDigitalRetailer _youTubeRetailer = new() { Id = (int)DigitalRetailer.YouTube, Name = "YouTube" };
    private readonly int _pageNumber = 1;
    private readonly int _pageSize = 20;
    
    public GetMovieOwnershipQueryHandlerTests()
    {
        _handler = new GetMovieOwnershipQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnCollectionDto_WithAllMovies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var movies = new List<MovieSummary>
        {
            new() { MovieId = 1, Title = "The Matrix",  ReleaseDate = new DateOnly(1999, 3, 31),  PosterPath = "/matrix.jpg",    Formats = [_bluRayFormat] },
            new() { MovieId = 2, Title = "Inception",   ReleaseDate = new DateOnly(2010, 7, 16),  PosterPath = "/inception.jpg", Formats = [_dvdFormat] }
        };

        _userRepositoryMock
            .Setup(r => r.GetUserMoviesAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MovieSummaryCollection { Movies = movies, TotalResults = movies.Count });

        var query = new GetMovieOwnershipQuery(userId, _pageNumber, _pageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Movies.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyCollection_WhenUserHasNoMovies()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(r => r.GetUserMoviesAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MovieSummaryCollection());

        // Act
        var result = await _handler.Handle(new GetMovieOwnershipQuery(userId, _pageNumber, _pageSize), CancellationToken.None);

        // Assert
        result.Movies.Should().BeEmpty();
        result.TotalOwnedCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_MapFormatAndRetailerCounts_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var movies = new List<MovieSummary>
        {
            new() { MovieId = 1, Title = "A", ReleaseDate = new DateOnly(2020, 1, 1), PosterPath = "/a.jpg",
                    Formats = [_dvdFormat, _bluRayFormat], DigitalRetailers = [_appleTvRetailer] },
            new() { MovieId = 2, Title = "B", ReleaseDate = new DateOnly(2021, 1, 1), PosterPath = "/b.jpg",
                    Formats = [_bluRay4KFormat] },
            new() { MovieId = 3, Title = "C", ReleaseDate = new DateOnly(2022, 1, 1), PosterPath = "/c.jpg",
                    Formats = [_dvdFormat], DigitalRetailers = [_moviesAnywhereRetailer, _youTubeRetailer] }
        };

        _userRepositoryMock
            .Setup(r => r.GetUserMoviesAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MovieSummaryCollection{Movies = movies, TotalResults = movies.Count });

        // Act
        var result = await _handler.Handle(new GetMovieOwnershipQuery(userId, _pageNumber, _pageSize), CancellationToken.None);

        // Assert
        result.TotalResults.Should().Be(3);
    }

    [Fact]
    public async Task Handle_Should_PassUserIdToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(r => r.GetUserMoviesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MovieSummaryCollection());

        // Act
        await _handler.Handle(new GetMovieOwnershipQuery(userId, _pageNumber, _pageSize), CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(r => r.GetUserMoviesAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

