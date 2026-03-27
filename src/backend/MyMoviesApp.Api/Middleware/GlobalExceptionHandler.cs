using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyMoviesApp.Domain.Exceptions;
using Polly.CircuitBreaker;

namespace MyMoviesApp.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is DuplicateEmailException)
        {
            logger.LogWarning("Duplicate email registration attempt. Path: {Path}", httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = exception.Message
            }, cancellationToken);

            return true;
        }

        if (exception is BrokenCircuitException)
        {
            logger.LogWarning(exception, "Circuit breaker open. TMDB service is unavailable. Path: {Path}", httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Service Unavailable",
                Detail = "A downstream service is currently unavailable. Please try again later."
            }, cancellationToken);

            return true;
        }

        if (exception is HttpRequestException httpEx && httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(exception, "Upstream TMDB service returned 401 Unauthorized. Path: {Path}", httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status502BadGateway;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = "Bad Gateway",
                Detail = "The upstream movie service is unavailable. Please try again later."
            }, cancellationToken);

            return true;
        }

        if (exception is UnauthorizedAccessException)
        {
            logger.LogWarning("Unauthorized access attempt. Path: {Path}", httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "You are not authorized to perform this action."
            }, cancellationToken);

            return true;
        }

        logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        return false;
    }
}

