using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.Auth.Commands;

namespace MyMoviesApp.Application.Tests.Features.Auth.Commands;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly Mock<ILogger<LoginUserCommandHandler>> _logger = new();
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _handler = new LoginUserCommandHandler(_authServiceMock.Object, _logger.Object);
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
        result.UserId.Should().Be(userId);
        result.Token.Should().Be(token);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessException_WhenCredentialsAreInvalid()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ValueTuple<Domain.Entities.User, string>?)null);

        var command = new LoginUserCommand("user@example.com", "wrongpassword");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_Should_CallLoginAsync_WithCorrectArguments()
    {
        // Arrange
        var user = new Domain.Entities.User(Guid.NewGuid(), "specific@example.com");

        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, "token"));

        var command = new LoginUserCommand("specific@example.com", "specificpassword");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authServiceMock.Verify(
            s => s.LoginAsync("specific@example.com", "specificpassword", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

