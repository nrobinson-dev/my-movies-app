using MediatR;
using Microsoft.Extensions.Logging;
using MyMoviesApp.Application.Common.Interfaces;

namespace MyMoviesApp.Application.Features.User.Commands;

public record DeleteMovieCommand(Guid UserId, int TmdbMovieId) : IRequest<Unit>;

public class DeleteMovieCommandHandler(IUserRepository userRepository, ILogger<DeleteMovieCommandHandler> logger) : IRequestHandler<DeleteMovieCommand, Unit>
{
    public async Task<Unit> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        await userRepository.DeleteUserMovieAsync(request.UserId, request.TmdbMovieId, cancellationToken);

        logger.LogInformation("Movie ownership deleted. UserId: {UserId}, TmdbMovieId: {TmdbMovieId}", request.UserId, request.TmdbMovieId);
        
        return Unit.Value;
    }
}