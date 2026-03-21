using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyMoviesApp.Api.Models;
using MyMoviesApp.Application.Features.Movies.Queries;
using MyMoviesApp.Application.Common.Dtos;
using System.Net;

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
            try
            {
                var result = await Mediator.Send(new GetMovieSearchResultsQuery(request.Search ?? string.Empty, request.UserId, request.Page ?? "1"), cancellationToken);
                return Ok(result);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { Message = "The upstream movie service is unavailable. Please try again later." });
            }
        }
    }
}
