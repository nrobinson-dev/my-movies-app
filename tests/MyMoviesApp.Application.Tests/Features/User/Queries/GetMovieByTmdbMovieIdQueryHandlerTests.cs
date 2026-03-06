using FluentAssertions;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.User.Queries;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Tests.Features.User.Queries;

public class GetMovieByTmdbMovieIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ITmdbService> _tmdbServiceMock = new();
    private readonly GetMovieByTmdbMovieIdQueryHandler _handler;
    private readonly UserMovieFormat _bluRayFormat = new() { Id = (int)Format.BluRay, Name = "Blu-ray" };
    private readonly UserMovieFormat _bluRay4KFormat = new() { Id = (int)Format.BluRay4K, Name = "Blu-ray 4K" };
    private readonly UserMovieDigitalRetailer _moviesAnywhereRetailer = new() { Id = (int)DigitalRetailer.MoviesAnywhere, Name = "Movies Anywhere" };
    
    public GetMovieByTmdbMovieIdQueryHandlerTests()
    {
        _handler = new GetMovieByTmdbMovieIdQueryHandler(_userRepositoryMock.Object, _tmdbServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnMovieDetailDto_WithFormatsAndRetailers()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var movieId = 123;

        var movieDetail = new MovieDetail(
            movieId, "The Matrix", new DateOnly(1999, 3, 31),
            136, "/matrix.jpg", "/matrix-backdrop.jpg",
            "Free your mind.", "A computer hacker learns the truth.");

        var formatsAndRetailers = new UserMovieFormatsAndDigitalRetailers
        {
            Formats = [_bluRayFormat, _bluRay4KFormat],
            DigitalRetailers = [_moviesAnywhereRetailer]
        };

        _tmdbServiceMock
            .Setup(s => s.GetMovieByTmdbMovieIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movieDetail);

        _userRepositoryMock
            .Setup(r => r.GetUserMovieFormatsAndDigitalRetailersAsync(userId, movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(formatsAndRetailers);

        var query = new GetMovieByTmdbMovieIdQuery(userId, movieId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TmdbId.Should().Be(movieId);
        result.Title.Should().Be("The Matrix");
        result.ReleaseDate.Should().Be(new DateOnly(1999, 3, 31));
        result.Runtime.Should().Be(136);
        result.PosterPath.Should().Be("/matrix.jpg");
        result.BackdropPath.Should().Be("/matrix-backdrop.jpg");
        result.Tagline.Should().Be("Free your mind.");
        result.Overview.Should().Be("A computer hacker learns the truth.");
        result.Formats.Should().Contain(_bluRayFormat).And.Contain(_bluRay4KFormat);
        result.DigitalRetailers.Should().Contain(_moviesAnywhereRetailer);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyFormatsAndRetailers_WhenUserHasNoOwnership()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var movieId = 456;

        var movieDetail = new MovieDetail(
            movieId, "Dune", new DateOnly(2021, 10, 22),
            155, "/dune.jpg", "/dune-backdrop.jpg",
            "Beyond fear, destiny awaits.", "A mythic journey.");

        _tmdbServiceMock
            .Setup(s => s.GetMovieByTmdbMovieIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movieDetail);

        _userRepositoryMock
            .Setup(r => r.GetUserMovieFormatsAndDigitalRetailersAsync(userId, movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserMovieFormatsAndDigitalRetailers());

        // Act
        var result = await _handler.Handle(new GetMovieByTmdbMovieIdQuery(userId, movieId), CancellationToken.None);

        // Assert
        result.Formats.Should().BeEmpty();
        result.DigitalRetailers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_CallBothServicesInParallel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var movieId = 789;

        _tmdbServiceMock
            .Setup(s => s.GetMovieByTmdbMovieIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MovieDetail(movieId, "Test", new DateOnly(2020, 1, 1), 100, "/p.jpg", "/b.jpg", "tag", "overview"));

        _userRepositoryMock
            .Setup(r => r.GetUserMovieFormatsAndDigitalRetailersAsync(userId, movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserMovieFormatsAndDigitalRetailers());

        // Act
        await _handler.Handle(new GetMovieByTmdbMovieIdQuery(userId, movieId), CancellationToken.None);

        // Assert — both services must have been called exactly once
        _tmdbServiceMock.Verify(s => s.GetMovieByTmdbMovieIdAsync(movieId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserMovieFormatsAndDigitalRetailersAsync(userId, movieId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

