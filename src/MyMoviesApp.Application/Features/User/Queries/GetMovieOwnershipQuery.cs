using MediatR;
using MyMoviesApp.Application.Common.Dtos;
using MyMoviesApp.Application.Common.Interfaces;

namespace MyMoviesApp.Application.Features.User.Queries;

public record GetMovieOwnershipQuery(Guid UserId) : IRequest<MovieSummaryCollectionDto>
{
    // TODO: Add pagination

    // public int PageNumber { get; init; } = 1;
    // public int PageSize { get; init; } = 10;
}

public class GetMovieOwnershipQueryHandler(IUserRepository userRepository) : IRequestHandler<GetMovieOwnershipQuery, MovieSummaryCollectionDto>
{
    public async Task<MovieSummaryCollectionDto> Handle(GetMovieOwnershipQuery request, CancellationToken cancellationToken)
    {
        var movies = await userRepository.GetUserMoviesAsync(request.UserId, cancellationToken);
        
        return new MovieSummaryCollectionDto(movies.Movies.Select(MovieSummaryDto.FromDomain));
    }
}