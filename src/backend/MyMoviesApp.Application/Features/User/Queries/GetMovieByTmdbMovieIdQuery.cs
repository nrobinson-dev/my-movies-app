using MediatR;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Common.Models;
using MyMoviesApp.Application.Features.Movies.Dtos;

namespace MyMoviesApp.Application.Features.User.Queries;

public record GetMovieByTmdbMovieIdQuery(Guid UserId, int MovieId) : IRequest<MovieDetailDto>;

public class GetMovieByTmdbMovieIdQueryHandler(IUserRepository userRepository, ITmdbService tmdbService) : IRequestHandler<GetMovieByTmdbMovieIdQuery, MovieDetailDto>
{
    public async Task<MovieDetailDto> Handle(GetMovieByTmdbMovieIdQuery request, CancellationToken cancellationToken)
    {
        var formatsAndRetailersTask = userRepository.GetUserMovieFormatsAndDigitalRetailersAsync(request.UserId, request.MovieId, cancellationToken);
        var movieDetailTask = tmdbService.GetMovieByTmdbMovieIdAsync(request.MovieId, cancellationToken);

        await Task.WhenAll(formatsAndRetailersTask, movieDetailTask);

        return new MovieDetailDto(movieDetailTask.Result)
        {
            Formats = formatsAndRetailersTask.Result.Formats
                .Select(f => new UserMovieFormatItem { Id = f.Id, Name = f.Name })
                .ToList(),
            DigitalRetailers = formatsAndRetailersTask.Result.DigitalRetailers
                .Select(r => new UserMovieDigitalRetailerItem { Id = r.Id, Name = r.Name })
                .ToList()
        };
    }
}