using FluentAssertions;
using Moq;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Exceptions;
using MyMoviesApp.Infrastructure.Services;

namespace MyMoviesApp.Infrastructure.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _service = new AuthenticationService(
            _userRepositoryMock.Object,
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

        _userRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(h => h.HashPassword(password))
            .Returns(hash);

        _userRepositoryMock
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

        _userRepositoryMock
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

        _userRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock.Setup(h => h.HashPassword(password)).Returns(hash);
        _userRepositoryMock.Setup(r => r.CreateUserAsync(It.IsAny<User>(), hash, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _jwtTokenGeneratorMock.Setup(g => g.GenerateJwtToken(It.IsAny<User>())).Returns("token");

        // Act
        await _service.RegisterAsync("a@b.com", password);

        // Assert
        _passwordHasherMock.Verify(h => h.HashPassword(password), Times.Once);
        _userRepositoryMock.Verify(r => r.CreateUserAsync(It.IsAny<User>(), hash, It.IsAny<CancellationToken>()), Times.Once);
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

        _userRepositoryMock
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
        _userRepositoryMock
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

        _userRepositoryMock
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
        _userRepositoryMock
            .Setup(r => r.GetUserWithPasswordHashByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ValueTuple<User, string>?)null);

        // Act
        await _service.LoginAsync("nobody@example.com", "password");

        // Assert
        _jwtTokenGeneratorMock.Verify(g => g.GenerateJwtToken(It.IsAny<User>()), Times.Never);
    }
}

