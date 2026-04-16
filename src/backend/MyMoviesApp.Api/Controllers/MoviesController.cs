using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyMoviesApp.Api.Models;
using MyMoviesApp.Application.Features.Movies.Queries;
using MyMoviesApp.Application.Common.Dtos;
using System.Security.Claims;
using MyMoviesApp.Application.Features.Movies.Dtos;

namespace MyMoviesApp.Api.Controllers
{
    [ApiController]
    [EnableRateLimiting("user")]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MoviesController : BaseApiController
    {
        /// <summary>
        /// Search for movies by title.
        /// </summary>
        /// <remarks>Queries the TMDB API for movies matching the search term and annotates each result with the user's ownership data.</remarks>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(MovieSummaryCollectionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<MovieSummaryCollectionDto>> GetSearchResults([FromQuery] MovieSearchRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetCallerUserId(out var userId))
            {
                return Forbid();
            }

            try
            {
                var query = new GetMovieSearchResultsQuery(request.Search, userId, request.Page);
                
                var result = await Mediator.Send(query, cancellationToken);
                return Ok(result);
            }
            catch  (HttpRequestException)
            {
                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "Bad Gateway",
                    detail: "The upstream movie service is unavailable. Please try again later.");
            }
        }

        /// <summary>
        /// Get a single movie by TMDB id for the authenticated user.
        /// </summary>
        /// <remarks>
        /// Returns TMDB movie details enriched with the caller's ownership formats and digital retailers.
        /// </remarks>
        [Authorize]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MovieDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<MovieDetailDto>> GetMovieDetails(int id, CancellationToken cancellationToken)
        {
            if (!TryGetCallerUserId(out var userId))
            {
                return Forbid();
            }

            try
            {
                var query = new GetMovieByTmdbMovieIdQuery(userId, id);

                var result = await Mediator.Send(query, cancellationToken);
                return Ok(result);
            } 
            catch  (HttpRequestException)
            {
                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "Bad Gateway",
                    detail: "The upstream movie service is unavailable. Please try again later.");
            }
        }
        
        private bool TryGetCallerUserId(out Guid userId)
        {
            var callerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(callerIdStr, out userId);
        }
    }
}
