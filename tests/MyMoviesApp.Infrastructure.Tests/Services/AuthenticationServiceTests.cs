using FluentAssertions;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Exceptions;
using MyMoviesApp.Infrastructure.Services;

namespace MyMoviesApp.Infrastructure.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _service = new AuthenticationService(
            _authRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object);
    }

    // ── RegisterAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_Should_ReturnUserAndToken_WhenEmailIsNew()
    {
        // Arrange
        const string email = "new@example.com";
        const string password = "password123";
        const string hash = "hashed-password";
        const string token = "jwt-token";

        _authRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(h => h.HashPassword(password))
            .Returns(hash);

        _authRepositoryMock
            .Setup(r => r.CreateUserAsync(It.IsAny<User>(), hash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _jwtTokenGeneratorMock
            .Setup(g => g.GenerateJwtToken(It.IsAny<User>()))
            .Returns(token);

        // Act
        var (user, returnedToken) = await _service.RegisterAsync(email, password);

        // Assert
        user.Email.Should().Be(email);
        returnedToken.Should().Be(token);
    }

    [Fact]
    public async Task RegisterAsync_Should_ThrowDuplicateEmailException_WhenEmailAlreadyExists()
    {
        // Arrange
        const string email = "existing@example.com";
        var existingUser = new User(Guid.NewGuid(), email);

        _authRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var act = () => _service.RegisterAsync(email, "password");

        // Assert
        await act.Should().ThrowAsync<DuplicateEmailException>()
            .WithMessage($"*{email}*");
    }

    [Fact]
    public async Task RegisterAsync_Should_HashPassword_BeforePersisting()
    {
        // Arrange
        const string password = "securepassword";
        const string hash = "secure-hash";

        _authRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock.Setup(h => h.HashPassword(password)).Returns(hash);
        _authRepositoryMock.Setup(r => r.CreateUserAsync(It.IsAny<User>(), hash, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _jwtTokenGeneratorMock.Setup(g => g.GenerateJwtToken(It.IsAny<User>())).Returns("token");

        // Act
        await _service.RegisterAsync("a@b.com", password);

        // Assert
        _passwordHasherMock.Verify(h => h.HashPassword(password), Times.Once);
        _authRepositoryMock.Verify(r => r.CreateUserAsync(It.IsAny<User>(), hash, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── LoginAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_Should_ReturnUserAndToken_WhenCredentialsAreValid()
    {
        // Arrange
        const string email = "user@example.com";
        const string password = "password123";
        const string storedHash = "stored-hash";
        const string token = "jwt-token";
        var user = new User(Guid.NewGuid(), email);

        _authRepositoryMock
            .Setup(r => r.GetUserWithPasswordHashByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, storedHash));

        _passwordHasherMock
            .Setup(h => h.VerifyPassword(password, storedHash))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(g => g.GenerateJwtToken(user))
            .Returns(token);

        // Act
        var result = await _service.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result!.Value.user.Email.Should().Be(email);
        result.Value.token.Should().Be(token);
    }

    [Fact]
    public async Task LoginAsync_Should_ReturnNull_WhenUserNotFound()
    {
        // Arrange
        _authRepositoryMock
            .Setup(r => r.GetUserWithPasswordHashByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ValueTuple<User, string>?)null);

        // Act
        var result = await _service.LoginAsync("ghost@example.com", "password");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_Should_ReturnNull_WhenPasswordIsWrong()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), "user@example.com");
        const string storedHash = "stored-hash";

        _authRepositoryMock
            .Setup(r => r.GetUserWithPasswordHashByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, storedHash));

        _passwordHasherMock
            .Setup(h => h.VerifyPassword("wrongpassword", storedHash))
            .Returns(false);

        // Act
        var result = await _service.LoginAsync("user@example.com", "wrongpassword");

        // Assert
        result.Should().BeNull();
        _jwtTokenGeneratorMock.Verify(g => g.GenerateJwtToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_Should_NotGenerateToken_WhenUserNotFound()
    {
        // Arrange
        _authRepositoryMock
            .Setup(r => r.GetUserWithPasswordHashByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ValueTuple<User, string>?)null);

        // Act
        await _service.LoginAsync("nobody@example.com", "password");

        // Assert
        _jwtTokenGeneratorMock.Verify(g => g.GenerateJwtToken(It.IsAny<User>()), Times.Never);
    }

    // ── DeleteUserAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteUserAsync_Should_CallRepository_WithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _authRepositoryMock
            .Setup(r => r.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.DeleteUserAsync(userId);

        // Assert
        _authRepositoryMock.Verify(r => r.DeleteUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_Should_ReturnRowsAffected_FromRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userFound = 1;

        _authRepositoryMock
            .Setup(r => r.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.DeleteUserAsync(userId);

        // Assert
        result.Should().Be(userFound);
    }

    [Fact]
    public async Task DeleteUserAsync_Should_ReturnZero_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userNotFound = 0;

        _authRepositoryMock
            .Setup(r => r.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _service.DeleteUserAsync(userId);

        // Assert
        result.Should().Be(userNotFound);
    }
}

