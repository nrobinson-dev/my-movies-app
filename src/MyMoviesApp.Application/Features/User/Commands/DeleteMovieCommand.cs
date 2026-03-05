using MediatR;
using MyMoviesApp.Application.Common.Interfaces;

namespace MyMoviesApp.Application.Features.User.Commands;

public record DeleteMovieCommand(Guid UserId, int TmdbMovieId) : IRequest<Unit>;

public class DeleteMovieCommandHandler(IUserRepository userRepository) : IRequestHandler<DeleteMovieCommand, Unit>
{
    public async Task<Unit> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        await userRepository.DeleteUserMovieAsync(request.UserId, request.TmdbMovieId, cancellationToken);
        
        return Unit.Value;
    }
}