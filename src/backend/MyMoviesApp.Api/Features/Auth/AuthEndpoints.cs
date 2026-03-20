using MyMoviesApp.Application.Features.User.Commands;
using MyMoviesApp.Application.Features.User.Dtos;
using MediatR;
using MyMoviesApp.Domain.Exceptions;

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
        .WithTags(nameof(Auth))
        .WithName("Register")
        .Produces<LoginUserResultDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status409Conflict);
        
        group.MapPost("/login", async (LoginUserCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            if (result == null)
            {
                return Results.Unauthorized();
            }
            return Results.Ok(result);
        })
        .DisableAntiforgery()
        .WithTags(nameof(Auth))
        .WithName("Login")
        .Produces(StatusCodes.Status200OK);
    }
}