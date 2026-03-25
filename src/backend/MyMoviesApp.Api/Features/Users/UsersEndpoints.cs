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
        var group = app.MapGroup("/api/users")
            .RequireRateLimiting("user");
        
        group.MapGet("{userId}/movies", async (Guid userId, IMediator mediator, CancellationToken cancellationToken, int page = 1, int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetMovieOwnershipQuery(userId, page, pageSize), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags(nameof(Users))
        .WithName("GetUserMovies")
        .WithSummary("Get a user's movie collection")
        .WithDescription("Returns a paginated list of movies owned by the specified user.")
        .Produces<MovieSummaryCollectionDto>()
        .RequireAuthorization();
        
        
        group.MapGet("{userId}/movies/{tmdbId}", async (Guid userId, int tmdbId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetMovieByTmdbMovieIdQuery(userId, tmdbId), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags(nameof(Users))
        .WithName("GetUserMoviesByTmdbMovieId")
        .WithSummary("Get a single user movie by TMDB ID")
        .WithDescription("Returns full details for one movie in the user's collection, enriched with live TMDB data.")
        .Produces<MovieDetailDto>()
        .Produces(StatusCodes.Status502BadGateway)
        .RequireAuthorization();
        
        
        group.MapPost("{userId}/movies", async (Guid userId, SaveUserMovieOwnershipDto request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(Map(userId, request), cancellationToken);
            return Results.Ok();
        })
        .WithTags(nameof(Users))
        .WithName("SaveUserMovieOwnership")
        .WithSummary("Save movie ownership for a user")
        .WithDescription("Adds or updates a movie in the user's collection with the specified formats and digital retailers.")
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
        .WithSummary("Delete movie ownership for a user")
        .WithDescription("Removes the specified movie from the user's collection.")
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
