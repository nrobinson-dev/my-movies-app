using MediatR;
using System.ComponentModel.DataAnnotations;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.User.Dtos;

namespace MyMoviesApp.Application.Features.User.Commands;

public record CreateUserCommand(
    [EmailAddress] string Email,
    string Password
) : IRequest<LoginUserResultDto>;

public class CreateUserCommandHandler(IAuthenticationService authenticationService) : IRequestHandler<CreateUserCommand, LoginUserResultDto>
{
    public async Task<LoginUserResultDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var (user, token) = await authenticationService.RegisterAsync(request.Email, request.Password, cancellationToken);
        
        var expiration = DateTime.UtcNow.AddHours(1);
        
        return new LoginUserResultDto(user.Id, token, expiration);
    }
}
