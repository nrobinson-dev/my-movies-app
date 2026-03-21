using MediatR;
using MyMoviesApp.Application.Common.Dtos;
using MyMoviesApp.Application.Common.Interfaces;

namespace MyMoviesApp.Application.Features.User.Queries;

public record GetMovieOwnershipQuery(Guid UserId, int? PageNumber, int? PageSize) : IRequest<MovieSummaryCollectionDto>
{
}

public class GetMovieOwnershipQueryHandler(IUserRepository userRepository) : IRequestHandler<GetMovieOwnershipQuery, MovieSummaryCollectionDto>
{
    public async Task<MovieSummaryCollectionDto> Handle(GetMovieOwnershipQuery request, CancellationToken cancellationToken)
    {
        var userMovieCollection = await userRepository.GetUserMoviesAsync(request.UserId, request.PageNumber ?? 1, request.PageSize ?? 20, cancellationToken);
        
        return new MovieSummaryCollectionDto(userMovieCollection.Movies.Select(MovieSummaryDto.FromDomain))
        {
            Page = userMovieCollection.Page,
            TotalPages = userMovieCollection.TotalPages,
            TotalResults = userMovieCollection.TotalResults,
        };
    }
}