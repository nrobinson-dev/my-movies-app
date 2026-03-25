using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Logging;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.Auth.Dtos;

namespace MyMoviesApp.Application.Features.Auth.Commands;

public record LoginUserCommand(
    [property: Required][property: EmailAddress] string? Email,
    [property: Required] string? Password
) : IRequest<LoginUserResultDto>;

public class LoginUserCommandHandler(IAuthenticationService authenticationService, ILogger<LoginUserCommandHandler> logger) : IRequestHandler<LoginUserCommand, LoginUserResultDto>
{
    public async Task<LoginUserResultDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var result = await authenticationService.LoginAsync(request.Email!, request.Password!, cancellationToken);

        if (result is null)
        {
            logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var (user, token) = result.Value;
        var expiration = DateTime.UtcNow.AddHours(1);

        logger.LogInformation("User logged in successfully. UserId: {UserId}", user.Id);

        return new LoginUserResultDto(user.Id, token, expiration);
    }
}