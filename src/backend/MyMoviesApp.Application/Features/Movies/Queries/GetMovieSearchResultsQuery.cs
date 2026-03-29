using MediatR;
using MyMoviesApp.Application.Common.Dtos;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Common.Models;
using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Application.Features.Movies.Queries;

public record GetMovieSearchResultsQuery(string Term, Guid? UserId, int Page = 1) : IRequest<TmdbMovieSummaryCollectionDto>;

public class GetMovieSearchResultsQueryHandler(ITmdbService tmdbService, IUserRepository userRepository) : IRequestHandler<GetMovieSearchResultsQuery, TmdbMovieSummaryCollectionDto>
{
    public async Task<TmdbMovieSummaryCollectionDto> Handle(GetMovieSearchResultsQuery request, CancellationToken cancellationToken)
    {
        var movieSummaryCollection = await tmdbService.SearchMoviesAsync(request.Term.ToLower().Trim(), cancellationToken, request.Page);

        Dictionary<int, MovieSummary>? userMoviesDictionary = null;

        if (request.UserId.HasValue)
        {
            var movieIds = movieSummaryCollection.Movies.Select(m => m.MovieId).ToList();
            var userMovies = await userRepository.GetUserMoviesByTmdbIdsAsync(request.UserId.Value, movieIds, cancellationToken);
            userMoviesDictionary = userMovies.ToDictionary(um => um.MovieId);
        }

        var dtos = movieSummaryCollection.Movies.Select(m => Map(MovieSummaryDto.FromDomain(m), userMoviesDictionary));

        return new TmdbMovieSummaryCollectionDto(dtos)
        {
            Page = movieSummaryCollection.Page,
            TotalPages = movieSummaryCollection.TotalPages,
            TotalResults = movieSummaryCollection.TotalResults,
        };
    }

    private static MovieSummaryDto Map(MovieSummaryDto dto, Dictionary<int, MovieSummary>? userMoviesDictionary)
    {
        if (userMoviesDictionary is not null && userMoviesDictionary.TryGetValue(dto.TmdbId, out var userMovie))
        {
            dto.Formats = userMovie.Formats
                .Select(f => new UserMovieFormatItem { Id = (int)f, Name = f.ToString() })
                .ToList();
            dto.DigitalRetailers = userMovie.DigitalRetailers
                .Select(r => new UserMovieDigitalRetailerItem { Id = (int)r, Name = r.ToString() })
                .ToList();
        }
        return dto;
    }
}