using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Infrastructure.Configuration;

namespace MyMoviesApp.Infrastructure.Services;

public class JwtTokenGenerator(IOptions<JwtOptions> jwtOptions) : IJwtTokenGenerator
{
    public string GenerateJwtToken(User user)
    {
        if (string.IsNullOrWhiteSpace(jwtOptions.Value.SigningKey))
            throw new InvalidOperationException("JwtSettings:SigningKey must be configured.");

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(jwtOptions.Value.AccessTokenExpirationMinutes > 0
            ? jwtOptions.Value.AccessTokenExpirationMinutes
            : 60);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SigningKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

