using MyMoviesApp.Application.Features.Auth.Commands;
using MyMoviesApp.Application.Features.Auth.Dtos;
using MediatR;
using System.Security.Claims;

namespace MyMoviesApp.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .RequireRateLimiting("auth");

        group.MapPost("/register", async (CreateUserCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/auth/register", result);
        })
        .DisableAntiforgery()
        //.AddEndpointFilter<ValidationFilter<CreateUserCommand>>()
        .WithTags(nameof(Auth))
        .WithName("Register")
        .WithSummary("Register a new user account")
        .WithDescription("Creates a new user account and returns a JWT access token on success.")
        .Produces<LoginUserResultDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);
        
        group.MapPost("/login", async (LoginUserCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .DisableAntiforgery()
        //.AddEndpointFilter<ValidationFilter<LoginUserCommand>>()
        .RequireRateLimiting("login")
        .WithTags(nameof(Auth))
        .WithName("Login")
        .WithSummary("Log in and receive a JWT")
        .WithDescription("Authenticates a user with email and password and returns a JWT access token.")
        .Produces<LoginUserResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status429TooManyRequests);
        
        group.MapDelete("/delete/{userId:guid}", async (Guid userId, ClaimsPrincipal caller, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var callerId = caller.FindFirstValue(ClaimTypes.NameIdentifier);
                if (callerId is null || !Guid.TryParse(callerId, out var callerGuid) || callerGuid != userId)
                    return Results.Forbid();

                await mediator.Send(new DeleteUserCommand(userId), cancellationToken);
                return Results.NoContent();
            })
            .WithTags(nameof(Auth))
            .WithName("DeleteUser")
            .WithSummary("Delete a user account")
            .WithDescription("Permanently deletes the authenticated user's account. The caller must match the userId in the route.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization();
    }
}