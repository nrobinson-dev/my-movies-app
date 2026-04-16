using MediatR;
using Microsoft.Extensions.Logging;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Common.Models;
using MyMoviesApp.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using MyMoviesApp.Application.Common.Services;

namespace MyMoviesApp.Application.Features.User.Commands;

public record SaveMovieOwnershipCommand(
    Guid UserId,
    [property: Range(1, int.MaxValue)] int TmdbId,
    [property: Required(AllowEmptyStrings = false)][property: StringLength(200, MinimumLength = 1)] string Title,
    DateOnly ReleaseDate,
    string PosterPath,
    HashSet<Format> Formats,
    HashSet<DigitalRetailer> DigitalRetailers
    ) : IRequest<int>;

public class SaveMovieOwnershipCommandHandler(
    IUserRepository userRepository, 
    ITitleFormattingService titleFormattingService,
    ILogger<SaveMovieOwnershipCommandHandler> logger) : IRequestHandler<SaveMovieOwnershipCommand, int>
{
    public async Task<int> Handle(SaveMovieOwnershipCommand request, CancellationToken cancellationToken)
    {
        var normalizedTitle = titleFormattingService.NormalizeForStorage(request.Title);
        
        var movieSummary = new SaveMovieSummary
        {
            MovieId = request.TmdbId,
            Title = normalizedTitle,
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
