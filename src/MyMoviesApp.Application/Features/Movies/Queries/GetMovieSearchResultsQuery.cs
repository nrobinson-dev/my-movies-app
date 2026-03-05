using MediatR;
using MyMoviesApp.Application.Common.Dtos;
using MyMoviesApp.Application.Common.Interfaces;

namespace MyMoviesApp.Application.Features.Movies.Queries;

public record GetMovieSearchResultsQuery(string Term, string Page = "1") : IRequest<MovieSummaryCollectionDto>;

public class GetMovieSearchResultsQueryHandler(ITmdbService tmdbService) : IRequestHandler<GetMovieSearchResultsQuery, MovieSummaryCollectionDto>
{
    public async Task<MovieSummaryCollectionDto> Handle(GetMovieSearchResultsQuery request, CancellationToken cancellationToken)
    {
        var movieSummaryCollection = await tmdbService.SearchMoviesAsync(request.Term, cancellationToken, request.Page);

        return new MovieSummaryCollectionDto(movieSummaryCollection.Movies.Select(MovieSummaryDto.FromDomain));
    }
}