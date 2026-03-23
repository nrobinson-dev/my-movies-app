using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace MyMoviesApp.Infrastructure.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is BrokenCircuitException)
        {
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
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "You are not authorized to perform this action."
            }, cancellationToken);

            return true;
        }

        return false;
    }
}