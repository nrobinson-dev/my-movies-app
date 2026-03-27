using MediatR;
using Microsoft.Extensions.Logging;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Common.Models;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Features.User.Commands;

public record SaveMovieOwnershipCommand(
    Guid UserId,
    int TmdbId,
    string Title,
    DateOnly ReleaseDate,
    string PosterPath,
    HashSet<Format> Formats,
    HashSet<DigitalRetailer> DigitalRetailers
    ) : IRequest<int>;

public class SaveMovieOwnershipCommandHandler(IUserRepository userRepository, ILogger<SaveMovieOwnershipCommandHandler> logger) : IRequestHandler<SaveMovieOwnershipCommand, int>
{
    public async Task<int> Handle(SaveMovieOwnershipCommand request, CancellationToken cancellationToken)
    {
        var movieSummary = new SaveMovieSummary
        {
            MovieId = request.TmdbId,
            Title = request.Title,
            ReleaseDate = request.ReleaseDate,
            PosterPath = request.PosterPath,
            Formats = request.Formats.ToList(),
            DigitalRetailers = request.DigitalRetailers.ToList()
        };
        
        var userMovieId = await userRepository.SaveUserMovieAsync(request.UserId, movieSummary, cancellationToken);

        logger.LogInformation("Movie ownership saved. UserId: {UserId}, TmdbId: {TmdbId}, Title: {Title}", request.UserId, request.TmdbId, request.Title);
        
        return userMovieId;
    }
}
