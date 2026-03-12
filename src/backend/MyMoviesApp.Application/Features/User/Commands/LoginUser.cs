using System.ComponentModel.DataAnnotations;
using MediatR;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.User.Dtos;

namespace MyMoviesApp.Application.Features.User.Commands;

public record LoginUserCommand(
    [EmailAddress] string Email,
    string Password
) : IRequest<LoginUserResultDto?>;

public class LoginUserCommandHandler(IAuthenticationService authenticationService) : IRequestHandler<LoginUserCommand, LoginUserResultDto?>
{
    public async Task<LoginUserResultDto?> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var result = await authenticationService.LoginAsync(request.Email, request.Password, cancellationToken);
        
        if (result is null)
            return null;

        var (user, token) = result.Value;
        return new LoginUserResultDto(user.Id, token);
    }
}