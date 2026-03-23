using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyMoviesApp.Api.Models;
using MyMoviesApp.Application.Features.Movies.Queries;
using MyMoviesApp.Application.Common.Dtos;

namespace MyMoviesApp.Api.Controllers
{
    [ApiController]
    [EnableRateLimiting("user")]
    [Route("api/[controller]")]
    public class MoviesController : BaseApiController
    {
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<MovieSummaryCollectionDto>> GetSearchResults([FromQuery] MovieSearchRequest request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetMovieSearchResultsQuery(request.Search ?? string.Empty, request.UserId, request.Page ?? "1"), cancellationToken);
            return Ok(result);
        }
    }
}
