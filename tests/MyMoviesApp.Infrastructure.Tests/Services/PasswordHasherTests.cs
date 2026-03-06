using FluentAssertions;
using MyMoviesApp.Infrastructure.Services;

namespace MyMoviesApp.Infrastructure.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_Should_ReturnNonEmptyString()
    {
        var hash = _hasher.HashPassword("mypassword");
        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void HashPassword_Should_ReturnDifferentHashes_ForSamePassword()
    {
        // Each call uses a new random salt
        var hash1 = _hasher.HashPassword("samepassword");
        var hash2 = _hasher.HashPassword("samepassword");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void HashPassword_Should_ReturnBase64String()
    {
        var hash = _hasher.HashPassword("somepassword");

        var act = () => Convert.FromBase64String(hash);
        act.Should().NotThrow();
    }

    [Fact]
    public void VerifyPassword_Should_ReturnTrue_WhenPasswordMatchesHash()
    {
        const string password = "correctpassword";
        var hash = _hasher.HashPassword(password);

        var result = _hasher.VerifyPassword(password, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_ReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        var hash = _hasher.HashPassword("originalpassword");

        var result = _hasher.VerifyPassword("wrongpassword", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_Should_ReturnFalse_ForEmptyPassword_AgainstValidHash()
    {
        var hash = _hasher.HashPassword("somepassword");

        var result = _hasher.VerifyPassword(string.Empty, hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_Should_ReturnFalse_WhenHashIsInvalidBase64()
    {
        var result = _hasher.VerifyPassword("password", "not-valid-base64!!!");

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_Should_ReturnFalse_WhenHashIsTooShort()
    {
        // Valid base64 but fewer bytes than expected (< SaltSize + HashSize)
        var tooShort = Convert.ToBase64String(new byte[10]);

        var result = _hasher.VerifyPassword("password", tooShort);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("password1")]
    [InlineData("P@ssw0rd!")]
    [InlineData("a")]
    [InlineData("a very long password with spaces and symbols !@#$%^&*()")]
    public void HashAndVerify_RoundTrip_Should_Succeed(string password)
    {
        var hash = _hasher.HashPassword(password);
        var result = _hasher.VerifyPassword(password, hash);
        result.Should().BeTrue();
    }
}

