using MyMoviesApp.Application.Features.Auth.Commands;
using MyMoviesApp.Application.Features.Auth.Dtos;
using MediatR;
using MyMoviesApp.Domain.Exceptions;
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
            try
            {
                var result = await mediator.Send(command, cancellationToken);
                return Results.Created($"/api/v1/auth/register", result);
            }
            catch (DuplicateEmailException ex)
            {
                return Results.Conflict(new { ex.Message });
            }
        })
        .DisableAntiforgery()
        .AddEndpointFilter<ValidationFilter<CreateUserCommand>>()
        .WithTags(nameof(Auth))
        .WithName("Register")
        .Produces<LoginUserResultDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);
        
        group.MapPost("/login", async (LoginUserCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .DisableAntiforgery()
        .AddEndpointFilter<ValidationFilter<LoginUserCommand>>()
        .RequireRateLimiting("login")
        .WithTags(nameof(Auth))
        .WithName("Login")
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
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization();
    }
}