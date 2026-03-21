using FluentAssertions;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.Auth.Commands;
using MyMoviesApp.Domain.Exceptions;

namespace MyMoviesApp.Application.Tests.Features.Auth.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _handler = new CreateUserCommandHandler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnLoginUserResultDto_WhenRegistrationSucceeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new Domain.Entities.User(userId, "test@example.com");
        var token = "jwt-token";

        _authServiceMock
            .Setup(s => s.RegisterAsync("test@example.com", "password123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, token));

        var command = new CreateUserCommand("test@example.com", "password123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Token.Should().Be(token);
    }

    [Fact]
    public async Task Handle_Should_ThrowDuplicateEmailException_WhenEmailAlreadyExists()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.RegisterAsync("dupe@example.com", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEmailException("dupe@example.com"));

        var command = new CreateUserCommand("dupe@example.com", "password123");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DuplicateEmailException>();
    }

    [Fact]
    public async Task Handle_Should_CallRegisterAsync_WithCorrectArguments()
    {
        // Arrange
        var user = new Domain.Entities.User(Guid.NewGuid(), "user@example.com");
        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, "token"));

        var command = new CreateUserCommand("user@example.com", "mypassword");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authServiceMock.Verify(
            s => s.RegisterAsync("user@example.com", "mypassword", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

