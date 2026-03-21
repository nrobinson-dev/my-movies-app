using MediatR;
using MyMoviesApp.Application.Common.Interfaces;

namespace MyMoviesApp.Application.Features.Auth.Commands;

public record DeleteUserCommand(Guid UserId) : IRequest<Unit>;

public class DeleteUserCommandHandler(IAuthenticationService authenticationService) : IRequestHandler<DeleteUserCommand, Unit>
{
    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await authenticationService.DeleteUserAsync(request.UserId, cancellationToken);
        return Unit.Value;
    }
}