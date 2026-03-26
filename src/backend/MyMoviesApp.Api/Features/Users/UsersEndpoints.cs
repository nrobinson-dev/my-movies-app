using MyMoviesApp.Application.Common.Dtos;
using MyMoviesApp.Application.Features.Movies.Dtos;
using MyMoviesApp.Application.Features.User.Commands;
using MyMoviesApp.Application.Features.User.Dtos;
using MyMoviesApp.Application.Features.User.Queries;
using MediatR;
using System.Security.Claims;

namespace MyMoviesApp.Api.Features.Users;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users")
            .RequireRateLimiting("user");
        
        group.MapGet("{userId}/movies", async (Guid userId, ClaimsPrincipal caller, IMediator mediator, CancellationToken cancellationToken, int page = 1, int pageSize = 20) =>
        {
            if (!IsCallerAuthorized(caller, userId)) return Results.Forbid();
            var safePage = Math.Max(1, page);
            var safePageSize = Math.Clamp(pageSize, 1, 100);
            var result = await mediator.Send(new GetMovieOwnershipQuery(userId, safePage, safePageSize), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags(nameof(Users))
        .WithName("GetUserMovies")
        .WithSummary("Get a user's movie collection")
        .WithDescription("Returns a paginated list of movies owned by the specified user.")
        .Produces<MovieSummaryCollectionDto>()
        .Produces(StatusCodes.Status403Forbidden)
        .RequireAuthorization();
        
        
        group.MapGet("{userId}/movies/{tmdbId}", async (Guid userId, int tmdbId, ClaimsPrincipal caller, IMediator mediator, CancellationToken cancellationToken) =>
        {
            if (!IsCallerAuthorized(caller, userId)) return Results.Forbid();
            var result = await mediator.Send(new GetMovieByTmdbMovieIdQuery(userId, tmdbId), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags(nameof(Users))
        .WithName("GetUserMoviesByTmdbMovieId")
        .WithSummary("Get a single user movie by TMDB ID")
        .WithDescription("Returns full details for one movie in the user's collection, enriched with live TMDB data.")
        .Produces<MovieDetailDto>()
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status502BadGateway)
        .RequireAuthorization();
        
        
        group.MapPost("{userId}/movies", async (Guid userId, SaveUserMovieOwnershipDto request, ClaimsPrincipal caller, IMediator mediator, CancellationToken cancellationToken) =>
        {
            if (!IsCallerAuthorized(caller, userId)) return Results.Forbid();
            await mediator.Send(Map(userId, request), cancellationToken);
            return Results.Ok();
        })
        .WithTags(nameof(Users))
        .WithName("SaveUserMovieOwnership")
        .WithSummary("Save movie ownership for a user")
        .WithDescription("Adds or updates a movie in the user's collection with the specified formats and digital retailers.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .ProducesValidationProblem()
        .AddEndpointFilter<ValidationFilter<SaveUserMovieOwnershipDto>>()
        .RequireAuthorization();
        
        
        group.MapDelete("{userId}/movies/{tmdbId}", async (Guid userId, int tmdbId, ClaimsPrincipal caller, IMediator mediator, CancellationToken cancellationToken) =>
        {
            if (!IsCallerAuthorized(caller, userId)) return Results.Forbid();
            await mediator.Send(new DeleteMovieCommand(userId, tmdbId), cancellationToken);
            return Results.NoContent();
        })
        .WithTags(nameof(Users))
        .WithName("DeleteUserMovieOwnership")
        .WithSummary("Delete movie ownership for a user")
        .WithDescription("Removes the specified movie from the user's collection.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status403Forbidden)
        .RequireAuthorization();
    }

    private static bool IsCallerAuthorized(ClaimsPrincipal caller, Guid userId)
    {
        var callerId = caller.FindFirstValue(ClaimTypes.NameIdentifier);
        return callerId is not null
               && Guid.TryParse(callerId, out var callerGuid)
               && callerGuid == userId;
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
