using MediatR;
using System.ComponentModel.DataAnnotations;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Features.Auth.Dtos;

namespace MyMoviesApp.Application.Features.Auth.Commands;

public record CreateUserCommand(
    [property: Required][property: EmailAddress] string? Email,
    [property: Required][property: MinLength(8)] string? Password
) : IRequest<LoginUserResultDto>;

public class CreateUserCommandHandler(IAuthenticationService authenticationService) : IRequestHandler<CreateUserCommand, LoginUserResultDto>
{
    public async Task<LoginUserResultDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var (user, token) = await authenticationService.RegisterAsync(request.Email!, request.Password!, cancellationToken);
        
        var expiration = DateTime.UtcNow.AddHours(1);
        
        return new LoginUserResultDto(user.Id, token, expiration);
    }
}
