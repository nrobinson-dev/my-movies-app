using FluentAssertions;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.Auth.Commands;
using MediatR;

namespace MyMoviesApp.Application.Tests.Features.Auth.Commands;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _handler = new DeleteUserCommandHandler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnUnit_WhenUserIsDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _authServiceMock
            .Setup(s => s.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new DeleteUserCommand(userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
    }


    [Fact]
    public async Task Handle_Should_CallDeleteUserAsync_WithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _authServiceMock
            .Setup(s => s.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new DeleteUserCommand(userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authServiceMock.Verify(s => s.DeleteUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

