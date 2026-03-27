using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Infrastructure.Configuration;
using MyMoviesApp.Infrastructure.Services;

namespace MyMoviesApp.Infrastructure.Tests.Services;

public class JwtTokenGeneratorTests
{
    private const string ValidSigningKey = "super-secret-signing-key-for-tests-that-is-long-enough";

    private static JwtTokenGenerator CreateGenerator(
        string signingKey = ValidSigningKey,
        string issuer = "TestIssuer",
        string audience = "TestAudience",
        int expirationMinutes = 60)
    {
        var options = Options.Create(new JwtOptions
        {
            SigningKey = signingKey,
            Issuer = issuer,
            Audience = audience,
            AccessTokenExpirationMinutes = expirationMinutes
        });
        return new JwtTokenGenerator(options);
    }

    [Fact]
    public void GenerateJwtToken_Should_ReturnNonEmptyToken()
    {
        var generator = CreateGenerator();
        var user = new User(Guid.NewGuid(), "user@example.com");

        var token = generator.GenerateJwtToken(user);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateJwtToken_Should_ReturnValidJwt_ThatCanBeParsed()
    {
        var generator = CreateGenerator();
        var user = new User(Guid.NewGuid(), "user@example.com");

        var tokenString = generator.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(tokenString).Should().BeTrue();

        var token = handler.ReadJwtToken(tokenString);
        token.Should().NotBeNull();
    }

    [Fact]
    public void GenerateJwtToken_Should_EmbedSubClaim_WithUserId()
    {
        var generator = CreateGenerator();
        var userId = Guid.NewGuid();
        var user = new User(userId, "user@example.com");

        var tokenString = generator.GenerateJwtToken(user);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);
        token.Subject.Should().Be(userId.ToString());
    }

    [Fact]
    public void GenerateJwtToken_Should_EmbedEmailClaim()
    {
        var generator = CreateGenerator();
        var user = new User(Guid.NewGuid(), "claims@example.com");

        var tokenString = generator.GenerateJwtToken(user);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "claims@example.com");
    }

    [Fact]
    public void GenerateJwtToken_Should_SetCorrectIssuerAndAudience()
    {
        var generator = CreateGenerator(issuer: "MyIssuer", audience: "MyAudience");
        var user = new User(Guid.NewGuid(), "user@example.com");

        var tokenString = generator.GenerateJwtToken(user);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);
        token.Issuer.Should().Be("MyIssuer");
        token.Audiences.Should().Contain("MyAudience");
    }

    [Fact]
    public void GenerateJwtToken_Should_SetExpiryApproximately_ToConfiguredMinutes()
    {
        var generator = CreateGenerator(expirationMinutes: 30);
        var user = new User(Guid.NewGuid(), "user@example.com");

        var before = DateTime.UtcNow;
        var tokenString = generator.GenerateJwtToken(user);
        var after = DateTime.UtcNow;

        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);
        token.ValidTo.Should().BeCloseTo(before.AddMinutes(30), TimeSpan.FromSeconds(5));
        token.ValidTo.Should().BeOnOrAfter(before.AddMinutes(30).AddSeconds(-1));
        token.ValidTo.Should().BeOnOrBefore(after.AddMinutes(30).AddSeconds(1));
    }

    [Fact]
    public void GenerateJwtToken_Should_DefaultToSixtyMinuteExpiry_WhenExpirationIsZero()
    {
        var generator = CreateGenerator(expirationMinutes: 0);
        var user = new User(Guid.NewGuid(), "user@example.com");

        var before = DateTime.UtcNow;
        var tokenString = generator.GenerateJwtToken(user);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);
        token.ValidTo.Should().BeCloseTo(before.AddMinutes(60), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateJwtToken_Should_ThrowInvalidOperationException_WhenSigningKeyIsEmpty()
    {
        var generator = CreateGenerator(signingKey: string.Empty);
        var user = new User(Guid.NewGuid(), "user@example.com");

        var act = () => generator.GenerateJwtToken(user);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SigningKey*");
    }

    [Fact]
    public void GenerateJwtToken_Should_ThrowInvalidOperationException_WhenSigningKeyIsWhitespace()
    {
        var generator = CreateGenerator(signingKey: "   ");
        var user = new User(Guid.NewGuid(), "user@example.com");

        var act = () => generator.GenerateJwtToken(user);

        act.Should().Throw<InvalidOperationException>();
    }
}

