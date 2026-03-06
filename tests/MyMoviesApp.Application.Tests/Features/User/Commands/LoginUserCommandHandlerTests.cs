using FluentAssertions;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.User.Commands;

namespace MyMoviesApp.Application.Tests.Features.User.Commands;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _handler = new LoginUserCommandHandler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnLoginUserResultDto_WhenCredentialsAreValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new Domain.Entities.User(userId, "user@example.com");
        var token = "jwt-token";

        _authServiceMock
            .Setup(s => s.LoginAsync("user@example.com", "password123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, token));

        var command = new LoginUserCommand("user@example.com", "password123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Token.Should().Be(token);
    }

    [Fact]
    public async Task Handle_Should_ReturnNull_WhenCredentialsAreInvalid()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ValueTuple<Domain.Entities.User, string>?)null);

        var command = new LoginUserCommand("user@example.com", "wrongpassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_CallLoginAsync_WithCorrectArguments()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ValueTuple<Domain.Entities.User, string>?)null);

        var command = new LoginUserCommand("specific@example.com", "specificpassword");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authServiceMock.Verify(
            s => s.LoginAsync("specific@example.com", "specificpassword", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

