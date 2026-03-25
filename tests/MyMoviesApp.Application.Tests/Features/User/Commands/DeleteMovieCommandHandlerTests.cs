using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.User.Commands;

namespace MyMoviesApp.Application.Tests.Features.User.Commands;

public class DeleteMovieCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ILogger<DeleteMovieCommandHandler>> _logger = new();
    private readonly DeleteMovieCommandHandler _handler;

    public DeleteMovieCommandHandlerTests()
    {
        _handler = new DeleteMovieCommandHandler(_userRepositoryMock.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnUnitValue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock
            .Setup(r => r.DeleteUserMovieAsync(userId, 123, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteMovieCommand(userId, 123);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_Should_CallDeleteUserMovieAsync_WithCorrectArguments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tmdbMovieId = 456;

        _userRepositoryMock
            .Setup(r => r.DeleteUserMovieAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteMovieCommand(userId, tmdbMovieId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.DeleteUserMovieAsync(userId, tmdbMovieId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

