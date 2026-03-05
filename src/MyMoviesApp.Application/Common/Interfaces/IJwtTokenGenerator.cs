using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Application.Common.Interfaces;

/// <summary>
/// JWT token generator interface for creating JWT tokens based on user information. This is used for authentication and authorization in the application.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Method to generate a JWT token for a given user.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    string GenerateJwtToken(User user);
}