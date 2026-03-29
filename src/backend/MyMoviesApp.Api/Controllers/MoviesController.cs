using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyMoviesApp.Api.Models;
using MyMoviesApp.Application.Features.Movies.Queries;
using MyMoviesApp.Application.Common.Dtos;
using System.Security.Claims;

namespace MyMoviesApp.Api.Controllers
{
    [ApiController]
    [EnableRateLimiting("user")]
    [Route("api/[controller]")]
    public class MoviesController : BaseApiController
    {
        /// <summary>
        /// Search for movies by title.
        /// </summary>
        /// <remarks>Queries the TMDB API for movies matching the search term and annotates each result with the user's ownership data.</remarks>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<MovieSummaryCollectionDto>> GetSearchResults([FromQuery] MovieSearchRequest request, CancellationToken cancellationToken)
        {
            var callerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? userId = Guid.TryParse(callerIdStr, out var callerGuid) ? callerGuid : null;

            var query = new GetMovieSearchResultsQuery(request.Search ?? string.Empty, userId, request.Page ?? 1);
            
            var result = await Mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
