using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MyMoviesApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        private IMediator? _mediator;

        // This property is available to all derived controllers
        protected IMediator Mediator => _mediator ??= 
            HttpContext.RequestServices.GetRequiredService<IMediator>();
    }
}
