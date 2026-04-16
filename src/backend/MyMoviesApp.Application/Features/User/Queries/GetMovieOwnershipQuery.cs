using MediatR;
using MyMoviesApp.Application.Common.Dtos;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Application.Common.Services;

namespace MyMoviesApp.Application.Features.User.Queries;

public record GetMovieOwnershipQuery(Guid UserId, int? PageNumber, int? PageSize) : IRequest<MovieSummaryCollectionDto>
{
}

public class GetMovieOwnershipQueryHandler(
    IUserRepository userRepository, 
    ITitleFormattingService titleFormattingService) : IRequestHandler<GetMovieOwnershipQuery, MovieSummaryCollectionDto>
{
    public async Task<MovieSummaryCollectionDto> Handle(GetMovieOwnershipQuery request, CancellationToken cancellationToken)
    {
        var userMovieCollection = await userRepository.GetUserMoviesAsync(
            request.UserId, 
            request.PageNumber ?? 1, 
            request.PageSize ?? 20, 
            cancellationToken);
        
        return new MovieSummaryCollectionDto(userMovieCollection.Movies.Select(m => MovieSummaryDto.FromDomain(m, titleFormattingService)))
        {
            Page = userMovieCollection.Page,
            TotalPages = userMovieCollection.TotalPages,
            TotalResults = userMovieCollection.TotalResults,
            TotalDvdCount =  userMovieCollection.TotalDvdCount,
            TotalBluRayCount = userMovieCollection.TotalBluRayCount,
            TotalBluRay4KCount =  userMovieCollection.TotalBluRay4KCount,
            TotalDigitalCount =  userMovieCollection.TotalDigitalCount
        };
    }
}