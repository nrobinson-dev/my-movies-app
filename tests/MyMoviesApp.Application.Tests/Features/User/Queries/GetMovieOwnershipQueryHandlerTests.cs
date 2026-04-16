using FluentAssertions;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Common.Services;
using MyMoviesApp.Application.Features.User.Queries;
using MyMoviesApp.Application.Tests.Common;
using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Application.Tests.Features.User.Queries;

public class GetMovieOwnershipQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ITitleFormattingService> _titleFormattingServiceMock = new();
    private readonly GetMovieOwnershipQueryHandler _handler;
    
    private readonly int _pageNumber = TestConstants.Pagination.DefaultPageNumber;
    private readonly int _pageSize = TestConstants.Pagination.DefaultPageSize;
    
    public GetMovieOwnershipQueryHandlerTests()
    {
        _handler = new GetMovieOwnershipQueryHandler(_userRepositoryMock.Object,  _titleFormattingServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnCollectionDto_WithAllMovies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var movies = new List<MovieSummary>
        {
            new() { 
                MovieId = 1, 
                Title = "The Matrix",  
                ReleaseDate = new DateOnly(1999, 3, 31),  
                PosterPath = "/matrix.jpg",    
                Formats = [TestConstants.Formats.BluRay4K] },
            new()
            {
                MovieId = 2, 
                Title = "Inception",   
                ReleaseDate = new DateOnly(2010, 7, 16),  
                PosterPath = "/inception.jpg", 
                Formats = [TestConstants.Formats.BluRay] 
            }
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
        
        var query = new GetMovieOwnershipQuery(userId, TestConstants.Pagination.DefaultPageNumber, TestConstants.Pagination.DefaultPageSize);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Movies.Should().BeEmpty();
        result.TotalResults.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_MapFormatAndRetailerCounts_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var movies = new List<MovieSummary>
        {
            new() { 
                MovieId = 1, 
                Title = "A", 
                ReleaseDate = new DateOnly(2020, 1, 1), 
                PosterPath = "/a.jpg",
                Formats = [TestConstants.Formats.Dvd, TestConstants.Formats.BluRay], 
                DigitalRetailers = [TestConstants.Retailers.AppleTv] 
            },
            new() { 
                MovieId = 2, 
                Title = "B", 
                ReleaseDate = new DateOnly(2021, 1, 1), 
                PosterPath = "/b.jpg",
                Formats = [TestConstants.Formats.BluRay4K] 
            },
            new() { 
                MovieId = 3, 
                Title = "C", 
                ReleaseDate = new DateOnly(2022, 1, 1), 
                PosterPath = "/c.jpg",
                Formats = [TestConstants.Formats.Dvd], 
                DigitalRetailers = [TestConstants.Retailers.MoviesAnywhere, TestConstants.Retailers.MoviesAnywhere] 
            }
        };

        _userRepositoryMock
            .Setup(r => r.GetUserMoviesAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MovieSummaryCollection{Movies = movies, TotalResults = movies.Count });

        var query = new GetMovieOwnershipQuery(userId, TestConstants.Pagination.DefaultPageNumber, TestConstants.Pagination.DefaultPageNumber);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new GetMovieOwnershipQuery(userId, TestConstants.Pagination.DefaultPageNumber, TestConstants.Pagination.DefaultPageNumber);
        
        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(r => r.GetUserMoviesAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

