using MyMoviesApp.Application.Common.Dtos;
using MyMoviesApp.Application.Features.Movies.Dtos;
using MyMoviesApp.Application.Features.User.Commands;
using MyMoviesApp.Application.Features.User.Dtos;
using MyMoviesApp.Application.Features.User.Queries;
using MediatR;

namespace MyMoviesApp.Api.Features.Users;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/users")
            .RequireRateLimiting("user");
        
        
        group.MapGet("{userId}/movies", async (Guid userId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetMovieOwnershipQuery(userId), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags(nameof(Users))
        .WithName("GetUserMovies")
        .Produces<MovieSummaryCollectionDto>()
        .RequireAuthorization();
        
        
        group.MapGet("{userId}/movies/{tmdbId}", async (Guid userId, int tmdbId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetMovieByTmdbMovieIdQuery(userId, tmdbId), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags(nameof(Users))
        .WithName("GetUserMoviesByTmdbMovieId")
        .Produces<MovieDetailDto>()
        .RequireAuthorization();
        
        
        group.MapPost("{userId}/movies", async (Guid userId, SaveUserMovieOwnershipDto request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(Map(userId, request), cancellationToken);
            return Results.Ok();
        })
        .WithTags(nameof(Users))
        .WithName("SaveUserMovieOwnership")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .AddEndpointFilter<ValidationFilter<SaveUserMovieOwnershipDto>>()
        .RequireAuthorization();
        
        
        group.MapDelete("{userId}/movies/{tmdbId}", async (Guid userId, int tmdbId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteMovieCommand(userId, tmdbId), cancellationToken);
            return Results.Ok();
        })
        .WithTags(nameof(Users))
        .WithName("DeleteUserMovieOwnership")
        .Produces(StatusCodes.Status204NoContent)
        .RequireAuthorization();
    }

    private static SaveMovieOwnershipCommand Map(Guid userId, SaveUserMovieOwnershipDto dto)
    {
        return new SaveMovieOwnershipCommand(
            UserId: userId,
            TmdbId: dto.TmdbId,
            Title: dto.Title,
            ReleaseDate: dto.ReleaseDate,
            PosterPath: dto.PosterPath,
            Formats: dto.Formats,
            DigitalRetailers: dto.DigitalRetailers
        );
    }
}
