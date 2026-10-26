using Microsoft.AspNetCore.Mvc;

namespace SearchEngine.Api.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHealth() => Ok();
    }
}
