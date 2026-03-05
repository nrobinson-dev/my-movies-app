using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMoviesApp.Application.Features.Movies.Queries;
using MyMoviesApp.Application.Common.Dtos;
using MyMoviesApp.Presentation.WebAPI.Models;

namespace MyMoviesApp.Presentation.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MoviesController : BaseApiController
    {
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<MovieSummaryCollectionDto>> GetSearchResults([FromQuery] MovieSearchRequest request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetMovieSearchResultsQuery(request.Search ?? string.Empty, request.Page ?? "1"), cancellationToken);
            return Ok(result);
        }
    }
}
