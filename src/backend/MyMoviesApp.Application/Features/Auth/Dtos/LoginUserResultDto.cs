namespace MyMoviesApp.Application.Features.Auth.Dtos;

/// <summary>
/// User DTO returned after a successful login, containing the user's ID and authentication token.
/// </summary>
/// <param name="UserId"></param>
/// <param name="Token"></param>
/// <param name="Expiration"></param>
public record LoginUserResultDto(
    Guid UserId,
    string Token,
    DateTime Expiration
);