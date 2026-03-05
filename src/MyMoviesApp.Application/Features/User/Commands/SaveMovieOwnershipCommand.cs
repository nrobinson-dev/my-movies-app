using MediatR;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Domain.Enums;
using MyMoviesApp.Domain.Entities;

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

public class SaveMovieOwnershipCommandHandler(IUserRepository userRepository) : IRequestHandler<SaveMovieOwnershipCommand, int>
{
    public async Task<int> Handle(SaveMovieOwnershipCommand request, CancellationToken cancellationToken)
    {
        var movieSummary = new MovieSummary
        {
            MovieId = request.TmdbId,
            Title = request.Title,
            ReleaseDate = request.ReleaseDate,
            PosterPath = request.PosterPath,
            Formats = request.Formats.ToList(),
            DigitalRetailers = request.DigitalRetailers.ToList()
        };
        
        var userMovieId = await userRepository.SaveUserMovieAsync(request.UserId, movieSummary, cancellationToken);
        
        return userMovieId;
    }
}
