using MyMoviesApp.Application.Common.Dtos;
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
        
        group.MapGet("me/movies", async (
                ClaimsPrincipal caller, 
                IMediator mediator, 
                CancellationToken cancellationToken, 
                int page = 1, 
                int pageSize = 20) =>
        {
            if (!TryGetCallerUserId(caller, out var userId)) return Results.Forbid();
            
            var safePage = Math.Max(1, page);
            var safePageSize = Math.Clamp(pageSize, 1, 100);
            var result = await mediator.Send(new GetMovieOwnershipQuery(userId, safePage, safePageSize), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags(nameof(Users))
        .WithName("GetUserMovies")
        .WithSummary("Get the authenticated user's movie collection")
        .WithDescription("Returns a paginated list of movies owned by the authenticated user.")
        .Produces<MovieSummaryCollectionDto>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization();
        
        
        group.MapPost("me/movies", async (
                SaveUserMovieOwnershipDto request, 
                ClaimsPrincipal caller, 
                IMediator mediator, 
                CancellationToken cancellationToken) =>
        {
            if (!TryGetCallerUserId(caller, out var userId)) return Results.Forbid();
            
            await mediator.Send(Map(userId, request), cancellationToken);
            return Results.Ok();
        })
        .WithTags(nameof(Users))
        .WithName("SaveMyMovieOwnership")
        .WithSummary("Save movie ownership for the authenticated user")
        .WithDescription("Adds or updates a movie in the authenticated user's collection with the specified formats and digital retailers.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesValidationProblem()
        .AddEndpointFilter<ValidationFilter<SaveUserMovieOwnershipDto>>()
        .RequireAuthorization();
        
        
        group.MapDelete("me/movies/{tmdbId:int}", async (
                int tmdbId, 
                ClaimsPrincipal caller, 
                IMediator mediator, 
                CancellationToken cancellationToken) =>
        {
            if (!TryGetCallerUserId(caller, out var userId)) return Results.Forbid();
            
            await mediator.Send(new DeleteMovieCommand(userId, tmdbId), cancellationToken);
            return Results.NoContent();
        })
        .WithTags(nameof(Users))
        .WithName("DeleteMyMovieOwnership")
        .WithSummary("Delete movie ownership for the authenticated user")
        .WithDescription("Removes the specified movie from the authenticated user's collection.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization();
    }

    private static bool TryGetCallerUserId(ClaimsPrincipal caller, out Guid userId)
    {
        var callerId = caller.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(callerId, out userId);
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
