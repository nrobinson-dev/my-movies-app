using FluentAssertions;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.User.Commands;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Tests.Features.User.Commands;

public class SaveMovieOwnershipCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly SaveMovieOwnershipCommandHandler _handler;

    public SaveMovieOwnershipCommandHandlerTests()
    {
        _handler = new SaveMovieOwnershipCommandHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnUserMovieId_WhenSaveSucceeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock
            .Setup(r => r.SaveUserMovieAsync(userId, It.IsAny<SaveMovieSummary>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var command = new SaveMovieOwnershipCommand(
            userId,
            TmdbId: 123,
            Title: "The Matrix",
            ReleaseDate: new DateOnly(1999, 3, 31),
            PosterPath: "/matrix.jpg",
            Formats: new HashSet<Format> { Format.BluRay },
            DigitalRetailers: new HashSet<DigitalRetailer> { DigitalRetailer.MoviesAnywhere });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public async Task Handle_Should_BuildMovieSummary_WithCorrectData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SaveMovieSummary? capturedSummary = null;

        _userRepositoryMock
            .Setup(r => r.SaveUserMovieAsync(userId, It.IsAny<SaveMovieSummary>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, SaveMovieSummary, CancellationToken>((_, m, _) => capturedSummary = m)
            .ReturnsAsync(1);

        var command = new SaveMovieOwnershipCommand(
            userId,
            TmdbId: 456,
            Title: "Inception",
            ReleaseDate: new DateOnly(2010, 7, 16),
            PosterPath: "/inception.jpg",
            Formats: new HashSet<Format> { Format.BluRay4K, Format.Dvd },
            DigitalRetailers: new HashSet<DigitalRetailer> { DigitalRetailer.AppleTv, DigitalRetailer.YouTube });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedSummary.Should().NotBeNull();
        capturedSummary!.MovieId.Should().Be(456);
        capturedSummary.Title.Should().Be("Inception");
        capturedSummary.ReleaseDate.Should().Be(new DateOnly(2010, 7, 16));
        capturedSummary.PosterPath.Should().Be("/inception.jpg");
        capturedSummary.Formats.Should().Contain(Format.BluRay4K).And.Contain(Format.Dvd);
        capturedSummary.DigitalRetailers.Should().Contain(DigitalRetailer.AppleTv).And.Contain(DigitalRetailer.YouTube);
    }

    [Fact]
    public async Task Handle_Should_SaveWithEmptyFormatsAndRetailers_WhenNoneProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SaveMovieSummary? capturedSummary = null;

        _userRepositoryMock
            .Setup(r => r.SaveUserMovieAsync(userId, It.IsAny<SaveMovieSummary>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, SaveMovieSummary, CancellationToken>((_, m, _) => capturedSummary = m)
            .ReturnsAsync(1);

        var command = new SaveMovieOwnershipCommand(
            userId,
            TmdbId: 789,
            Title: "No Formats Movie",
            ReleaseDate: new DateOnly(2020, 1, 1),
            PosterPath: "/poster.jpg",
            Formats: new HashSet<Format>(),
            DigitalRetailers: new HashSet<DigitalRetailer>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedSummary!.Formats.Should().BeEmpty();
        capturedSummary.DigitalRetailers.Should().BeEmpty();
    }
}
