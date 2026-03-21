using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MyMoviesApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        // This property is available to all derived controllers
        protected IMediator Mediator => field ??= 
            HttpContext.RequestServices.GetRequiredService<IMediator>();
    }
}
